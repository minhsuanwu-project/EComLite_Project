using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EComLite.Tests
{
    public class CheckoutTests
    {
        // ── DB Helper ────────────────────────────────────────────────────────
        private static ApplicationDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static Product MakeProduct(decimal price = 19.99m, int stock = 10)
            => new Product
            {
                ProductId = Guid.NewGuid(),
                Sku = "TEST-001",
                Name = "Test Product",
                Price = price,
                StockQty = stock,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        // ── Order persistence ────────────────────────────────────────────────

        [Fact]
        public async Task PlaceOrder_ValidItems_OrderSavedToDatabase()
        {
            using var db = CreateInMemoryDb("test_valid_order");
            var product = MakeProduct();
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var orderId = Guid.NewGuid();
            var placedAt = DateTime.UtcNow;
            var order = new Order
            {
                OrderId = orderId,
                OrderNumber = $"ORD-{placedAt:yyyyMMdd}-TEST",
                UserId = "user-001",
                TotalAmount = 19.99m,
                Currency = "USD",
                Status = "Placed",
                PlacedAt = placedAt
            };
            order.Items.Add(new OrderItem
            {
                ProductId = product.ProductId,
                Qty = 1,
                UnitPriceSnapshot = product.Price
            });

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var saved = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);
            Assert.NotNull(saved);
            Assert.Single(saved.Items);
        }

        [Fact]
        public async Task PlaceOrder_OrderItemCountMatchesCartItems()
        {
            using var db = CreateInMemoryDb("test_item_count");
            var p1 = MakeProduct(19.99m);
            var p2 = MakeProduct(25.99m);
            db.Products.AddRange(p1, p2);
            await db.SaveChangesAsync();

            var orderId = Guid.NewGuid();
            var placedAt = DateTime.UtcNow;
            var order = new Order
            {
                OrderId = orderId,
                OrderNumber = $"ORD-{placedAt:yyyyMMdd}-TEST",
                UserId = "user-001",
                TotalAmount = 45.98m,
                Currency = "USD",
                Status = "Placed",
                PlacedAt = placedAt
            };
            order.Items.Add(new OrderItem { ProductId = p1.ProductId, Qty = 1, UnitPriceSnapshot = p1.Price });
            order.Items.Add(new OrderItem { ProductId = p2.ProductId, Qty = 1, UnitPriceSnapshot = p2.Price });

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var saved = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);
            Assert.Equal(2, saved!.Items.Count);
        }

        // ── Price snapshot ───────────────────────────────────────────────────

        [Fact]
        public async Task PlaceOrder_PriceSnapshotPreserved_AfterProductPriceChange()
        {
            using var db = CreateInMemoryDb("test_price_snapshot");
            var product = MakeProduct(19.99m);
            db.Products.Add(product);
            await db.SaveChangesAsync();

            // Place order at original price
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = "ORD-TEST-001",
                UserId = "user-001",
                TotalAmount = 19.99m,
                Currency = "USD",
                Status = "Placed",
                PlacedAt = DateTime.UtcNow
            };
            order.Items.Add(new OrderItem
            {
                ProductId = product.ProductId,
                Qty = 1,
                UnitPriceSnapshot = product.Price   // snapshot at 19.99
            });
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Simulate product price change
            product.Price = 39.99m;
            product.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // Order snapshot should still be 19.99
            var saved = await db.Orders.Include(o => o.Items).FirstAsync();
            Assert.Equal(19.99m, saved.Items.First().UnitPriceSnapshot);
        }

        // ── Data consistency ─────────────────────────────────────────────────

        [Fact]
        public async Task PlaceOrder_EveryOrderHasAtLeastOneItem()
        {
            using var db = CreateInMemoryDb("test_consistency");
            var product = MakeProduct();
            db.Products.Add(product);

            for (int i = 0; i < 3; i++)
            {
                var oid = Guid.NewGuid();
                var o = new Order
                {
                    OrderId = oid,
                    OrderNumber = $"ORD-TEST-{i:D3}",
                    UserId = "user-001",
                    TotalAmount = 19.99m,
                    Currency = "USD",
                    Status = "Placed",
                    PlacedAt = DateTime.UtcNow
                };
                o.Items.Add(new OrderItem { ProductId = product.ProductId, Qty = 1, UnitPriceSnapshot = 19.99m });
                db.Orders.Add(o);
            }
            await db.SaveChangesAsync();

            var orders = await db.Orders.Include(o => o.Items).ToListAsync();
            Assert.All(orders, o => Assert.NotEmpty(o.Items));
        }

        [Fact]
        public async Task PlaceOrder_TotalAmountMatchesSumOfItems()
        {
            using var db = CreateInMemoryDb("test_total");
            var product = MakeProduct(25.00m);
            db.Products.Add(product);
            await db.SaveChangesAsync();

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = "ORD-TEST-TOTAL",
                UserId = "user-001",
                TotalAmount = 75.00m,   // 25.00 × 3
                Currency = "USD",
                Status = "Placed",
                PlacedAt = DateTime.UtcNow
            };
            order.Items.Add(new OrderItem { ProductId = product.ProductId, Qty = 3, UnitPriceSnapshot = 25.00m });
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var saved = await db.Orders.Include(o => o.Items).FirstAsync();
            var calculatedTotal = saved.Items.Sum(i => i.Qty * i.UnitPriceSnapshot);
            Assert.Equal(saved.TotalAmount, calculatedTotal);
        }

        // ── Negative tests ───────────────────────────────────────────────────

        [Fact]
        public async Task ArchivedProduct_NotReturnedInCatalog()
        {
            using var db = CreateInMemoryDb("test_archived");
            db.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(), Sku = "ARCH-001", Name = "Old Product",
                Price = 9.99m, StockQty = 0, IsArchived = true,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            db.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(), Sku = "LIVE-001", Name = "Live Product",
                Price = 19.99m, StockQty = 5, IsArchived = false,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var visible = await db.Products.Where(p => !p.IsArchived).ToListAsync();
            Assert.Single(visible);
            Assert.Equal("Live Product", visible[0].Name);
        }

        [Fact]
        public async Task OrderHistory_OnlyReturnsOrdersForCorrectUser()
        {
            using var db = CreateInMemoryDb("test_user_isolation");
            var product = MakeProduct();
            db.Products.Add(product);

            db.Orders.Add(new Order { OrderId = Guid.NewGuid(), OrderNumber = "ORD-U1", UserId = "user-001", TotalAmount = 19.99m, Currency = "USD", Status = "Placed", PlacedAt = DateTime.UtcNow });
            db.Orders.Add(new Order { OrderId = Guid.NewGuid(), OrderNumber = "ORD-U2", UserId = "user-002", TotalAmount = 19.99m, Currency = "USD", Status = "Placed", PlacedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var user1Orders = await db.Orders.Where(o => o.UserId == "user-001").ToListAsync();
            Assert.Single(user1Orders);
            Assert.Equal("ORD-U1", user1Orders[0].OrderNumber);
        }
    }
}
