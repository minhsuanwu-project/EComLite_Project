using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using EComLite.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EComLite.Tests
{
    // UE-4.1-02: idempotent, duplicate-safe checkout.
    public class CheckoutServiceTests
    {
        private static ApplicationDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                // InMemory has no real transactions; allow BeginTransaction to be a no-op.
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private static List<CartItem> OneItem(decimal price = 20.00m, int qty = 2)
            => new()
            {
                new CartItem { ProductId = Guid.NewGuid(), ProductName = "P", UnitPrice = price, Qty = qty }
            };

        [Fact]
        public async Task SameIdempotencyKey_SubmittedTwice_CreatesOnlyOneOrder()
        {
            using var db = CreateDb("chk_same_key");
            var svc = new CheckoutService(db);
            var items = OneItem();

            var first = await svc.PlaceOrderIdempotentAsync("user-001", items, "key-1");
            var second = await svc.PlaceOrderIdempotentAsync("user-001", items, "key-1");

            Assert.True(first.Created);
            Assert.False(second.Created);
            Assert.Equal(first.Order.OrderId, second.Order.OrderId);
            Assert.Equal(1, await db.Orders.CountAsync(o => o.UserId == "user-001"));
        }

        [Fact]
        public async Task DifferentIdempotencyKeys_CreateTwoOrders()
        {
            using var db = CreateDb("chk_diff_keys");
            var svc = new CheckoutService(db);

            var a = await svc.PlaceOrderIdempotentAsync("user-001", OneItem(), "key-1");
            var b = await svc.PlaceOrderIdempotentAsync("user-001", OneItem(), "key-2");

            Assert.True(a.Created);
            Assert.True(b.Created);
            Assert.NotEqual(a.Order.OrderId, b.Order.OrderId);
            Assert.Equal(2, await db.Orders.CountAsync(o => o.UserId == "user-001"));
        }

        [Fact]
        public async Task PlacedOrder_HasCorrectTotalStatusAndKey()
        {
            using var db = CreateDb("chk_fields");
            var svc = new CheckoutService(db);
            var items = OneItem(price: 25.00m, qty: 3);

            var result = await svc.PlaceOrderIdempotentAsync("user-001", items, "key-9");

            Assert.Equal(75.00m, result.Order.TotalAmount);          // 25 * 3
            Assert.Equal(OrderStatus.Pending, result.Order.Status);   // new orders start Pending
            Assert.Equal("key-9", result.Order.IdempotencyKey);
            Assert.Single(result.Order.Items);
        }
    }
}
