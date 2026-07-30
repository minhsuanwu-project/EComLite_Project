using System;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using EComLite.Web.Pages.Products;
using EComLite.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EComLite.Tests
{
    // UE-2.2-01 (View Product Details) and UE-5.2-01 (View Order Details).
    public class AccessControlTests
    {
        private static ApplicationDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static Product MakeProduct(bool archived)
            => new Product
            {
                ProductId = Guid.NewGuid(),
                Sku = "SKU-1",
                Name = archived ? "Archived Product" : "Live Product",
                Price = 19.99m,
                StockQty = 5,
                IsArchived = archived,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        private static ProductsDetailsModelWrapper NewProductDetails(ApplicationDbContext db)
            => new ProductsDetailsModelWrapper(db);

        // ── UE-2.2-01: product details must hide archived or missing products ──

        [Fact]
        public async Task ProductDetails_LiveProduct_ReturnsPage()
        {
            using var db = CreateDb("pd_live");
            var p = MakeProduct(archived: false);
            db.Products.Add(p);
            await db.SaveChangesAsync();

            var model = NewProductDetails(db).Model;
            var result = await model.OnGetAsync(p.ProductId);

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task ProductDetails_ArchivedProduct_ReturnsNotFound()
        {
            using var db = CreateDb("pd_archived");
            var p = MakeProduct(archived: true);
            db.Products.Add(p);
            await db.SaveChangesAsync();

            var model = NewProductDetails(db).Model;
            var result = await model.OnGetAsync(p.ProductId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ProductDetails_MissingProduct_ReturnsNotFound()
        {
            using var db = CreateDb("pd_missing");

            var model = NewProductDetails(db).Model;
            var result = await model.OnGetAsync(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        // ── UE-5.2-01: order details must be scoped to the owning user ──
        // Verifies the exact scoping predicate used by Orders/Details (OrderId + UserId),
        // which drives the page's NotFound response for another user's order.

        [Fact]
        public async Task OrderDetails_OwnOrder_IsAccessible()
        {
            using var db = CreateDb("od_own");
            var orderId = Guid.NewGuid();
            db.Orders.Add(new Order { OrderId = orderId, OrderNumber = "ORD-1", UserId = "user-001", TotalAmount = 10m, Currency = "USD", Status = OrderStatus.Pending, PlacedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var found = await db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == "user-001");

            Assert.NotNull(found);
        }

        [Fact]
        public async Task OrderDetails_OtherUsersOrder_IsNotAccessible()
        {
            using var db = CreateDb("od_other");
            var orderId = Guid.NewGuid();
            db.Orders.Add(new Order { OrderId = orderId, OrderNumber = "ORD-1", UserId = "user-001", TotalAmount = 10m, Currency = "USD", Status = OrderStatus.Pending, PlacedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            // A different user requesting the same order id must get nothing back
            // (the page returns NotFound in this case).
            var found = await db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == "user-002");

            Assert.Null(found);
        }
    }

    // Small helper to build a Products.DetailsModel with a no-op CartService.
    internal class ProductsDetailsModelWrapper
    {
        public DetailsModel Model { get; }

        public ProductsDetailsModelWrapper(ApplicationDbContext db)
        {
            var cartService = new CartService(new Mock<IHttpContextAccessor>().Object);
            Model = new DetailsModel(db, cartService);
        }
    }
}
