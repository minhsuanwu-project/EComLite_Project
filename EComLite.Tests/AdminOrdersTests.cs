using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using EComLite.Web.Pages.Admin;
using EComLite.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EComLite.Tests
{
    // Version 2: Admin Order Management dashboard.
    // Covers R3 (privilege escalation on admin routes) and the wiring of the
    // status lifecycle into the only UI that can change an order's status.
    public class AdminOrdersTests
    {
        private static ApplicationDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static Order MakeOrder(string status, string user = "user-001")
            => new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = "ORD-TEST",
                UserId = user,
                TotalAmount = 10m,
                Currency = "USD",
                Status = status,
                PlacedAt = DateTime.UtcNow
            };

        // ── R3: the admin page must be role-gated server-side ──

        [Fact]
        public void AdminOrdersPage_RequiresAuthorization()
        {
            var attr = typeof(OrdersModel).GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(attr);
        }

        [Fact]
        public void AdminOrdersPage_RestrictsToAdminRole()
        {
            var attr = typeof(OrdersModel).GetCustomAttribute<AuthorizeAttribute>();
            Assert.Equal("Admin", attr!.Roles);
        }

        // ── Status changes go through OrderStatusService and are validated ──

        [Fact]
        public async Task AdvancingStatus_OneStepForward_IsApplied()
        {
            using var db = CreateDb("admin_advance_ok");
            var order = MakeOrder(OrderStatus.Pending);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = await new OrderStatusService(db)
                .ChangeStatusAsync(order.OrderId, OrderStatus.Processing);

            Assert.True(result.Success, result.Error);
            var saved = await db.Orders.FirstAsync(o => o.OrderId == order.OrderId);
            Assert.Equal(OrderStatus.Processing, saved.Status);
        }

        [Fact]
        public async Task AdvancingStatus_SkippingAStage_IsRejectedAndStatusUnchanged()
        {
            using var db = CreateDb("admin_advance_skip");
            var order = MakeOrder(OrderStatus.Pending);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = await new OrderStatusService(db)
                .ChangeStatusAsync(order.OrderId, OrderStatus.Delivered);

            Assert.False(result.Success);
            var saved = await db.Orders.FirstAsync(o => o.OrderId == order.OrderId);
            Assert.Equal(OrderStatus.Pending, saved.Status);   // unchanged
        }

        [Fact]
        public async Task ChangingStatus_OnMissingOrder_Fails()
        {
            using var db = CreateDb("admin_advance_missing");

            var result = await new OrderStatusService(db)
                .ChangeStatusAsync(Guid.NewGuid(), OrderStatus.Processing);

            Assert.False(result.Success);
        }

        // ── The dashboard offers exactly the one legal next step ──

        [Theory]
        [InlineData("Pending", "Processing")]
        [InlineData("Processing", "Shipped")]
        [InlineData("Shipped", "Delivered")]
        public void NextStatus_IsTheSingleAllowedForwardStep(string current, string expected)
        {
            Assert.Equal(expected, OrderStatus.AllowedNext(current).FirstOrDefault());
        }

        [Fact]
        public void NextStatus_ForDeliveredOrder_IsNone()
        {
            Assert.Null(OrderStatus.AllowedNext(OrderStatus.Delivered).FirstOrDefault());
        }
    }
}
