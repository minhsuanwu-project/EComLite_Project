using System;
using System.Collections.Generic;
using System.Text.Json;
using EComLite.Web.Models;
using EComLite.Web.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace EComLite.Tests
{
    public class CartServiceTests
    {
        // ── Helpers ─────────────────────────────────────────────────────────
        private static CartService CreateCartService(out Dictionary<string, byte[]> sessionStore)
        {
            var store = new Dictionary<string, byte[]>();
            sessionStore = store;

            var mockSession = new Mock<ISession>();

            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => store[key] = value);

            mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    var found = store.TryGetValue(key, out var stored);
                    value = stored ?? Array.Empty<byte>();
                    return found;
                });

            mockSession.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback<string>(key => store.Remove(key));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            return new CartService(mockAccessor.Object);
        }

        private static CartItem MakeItem(string name = "T-Shirt", decimal price = 19.99m, int qty = 1)
            => new CartItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = name,
                UnitPrice = price,
                Qty = qty
            };

        // ── GetCart ──────────────────────────────────────────────────────────

        [Fact]
        public void GetCart_WhenSessionEmpty_ReturnsEmptyList()
        {
            var svc = CreateCartService(out _);
            var result = svc.GetCart();
            Assert.Empty(result);
        }

        // ── AddItem ──────────────────────────────────────────────────────────

        [Fact]
        public void AddItem_NewProduct_AddsToCart()
        {
            var svc = CreateCartService(out _);
            var item = MakeItem();

            svc.AddItem(item);

            var cart = svc.GetCart();
            Assert.Single(cart);
            Assert.Equal(item.ProductName, cart[0].ProductName);
        }

        [Fact]
        public void AddItem_SameProductTwice_AccumulatesQty()
        {
            var svc = CreateCartService(out _);
            var productId = Guid.NewGuid();

            svc.AddItem(new CartItem { ProductId = productId, ProductName = "T-Shirt", UnitPrice = 19.99m, Qty = 2 });
            svc.AddItem(new CartItem { ProductId = productId, ProductName = "T-Shirt", UnitPrice = 19.99m, Qty = 3 });

            var cart = svc.GetCart();
            Assert.Single(cart);
            Assert.Equal(5, cart[0].Qty);
        }

        [Fact]
        public void AddItem_DifferentProducts_AddsBothToCart()
        {
            var svc = CreateCartService(out _);

            svc.AddItem(MakeItem("T-Shirt"));
            svc.AddItem(MakeItem("Hoodie"));

            var cart = svc.GetCart();
            Assert.Equal(2, cart.Count);
        }

        // ── Remove ───────────────────────────────────────────────────────────

        [Fact]
        public void Remove_ExistingProduct_RemovesFromCart()
        {
            var svc = CreateCartService(out _);
            var item = MakeItem();
            svc.AddItem(item);

            svc.Remove(item.ProductId);

            Assert.Empty(svc.GetCart());
        }

        [Fact]
        public void Remove_NonExistentProduct_CartUnchanged()
        {
            var svc = CreateCartService(out _);
            var item = MakeItem();
            svc.AddItem(item);

            svc.Remove(Guid.NewGuid());

            Assert.Single(svc.GetCart());
        }

        // ── Clear ────────────────────────────────────────────────────────────

        [Fact]
        public void Clear_WithItems_EmptiesCart()
        {
            var svc = CreateCartService(out _);
            svc.AddItem(MakeItem("T-Shirt"));
            svc.AddItem(MakeItem("Hoodie"));

            svc.Clear();

            Assert.Empty(svc.GetCart());
        }

        [Fact]
        public void Clear_EmptyCart_RemainsEmpty()
        {
            var svc = CreateCartService(out _);
            svc.Clear();
            Assert.Empty(svc.GetCart());
        }

        // ── Total calculation ────────────────────────────────────────────────

        [Fact]
        public void AddItem_PriceAndQty_TotalCalculatedCorrectly()
        {
            var svc = CreateCartService(out _);
            svc.AddItem(new CartItem { ProductId = Guid.NewGuid(), ProductName = "T-Shirt", UnitPrice = 19.99m, Qty = 3 });

            var cart = svc.GetCart();
            var total = 0m;
            foreach (var i in cart) total += i.Qty * i.UnitPrice;

            Assert.Equal(59.97m, total);
        }
    }
}
