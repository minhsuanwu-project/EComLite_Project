using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using EComLite.Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EComLite.Tests
{
    // UE-4.1-03: cart persisted to the database, keyed by user ID.
    public class PersistentCartServiceTests
    {
        private static ApplicationDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static List<CartItem> Cart(int qty = 2)
            => new()
            {
                new CartItem { ProductId = Guid.NewGuid(), ProductName = "P", UnitPrice = 9.99m, Qty = qty }
            };

        [Fact]
        public async Task Save_ThenLoad_ReturnsSameItems()
        {
            using var db = CreateDb("pc_save_load");
            var svc = new PersistentCartService(db);
            var items = Cart(qty: 3);

            await svc.SaveAsync("user-001", items);
            var loaded = await svc.LoadAsync("user-001");

            Assert.Single(loaded);
            Assert.Equal(items[0].ProductId, loaded[0].ProductId);
            Assert.Equal(3, loaded[0].Qty);
        }

        [Fact]
        public async Task Save_Twice_OverwritesAndKeepsOneRowPerUser()
        {
            using var db = CreateDb("pc_overwrite");
            var svc = new PersistentCartService(db);

            await svc.SaveAsync("user-001", Cart(qty: 1));
            await svc.SaveAsync("user-001", Cart(qty: 5));

            var loaded = await svc.LoadAsync("user-001");
            Assert.Equal(5, loaded[0].Qty);
            Assert.Equal(1, await db.PersistedCarts.CountAsync(c => c.UserId == "user-001"));
        }

        [Fact]
        public async Task Clear_RemovesPersistedCart()
        {
            using var db = CreateDb("pc_clear");
            var svc = new PersistentCartService(db);

            await svc.SaveAsync("user-001", Cart());
            await svc.ClearAsync("user-001");

            var loaded = await svc.LoadAsync("user-001");
            Assert.Empty(loaded);
        }

        [Fact]
        public async Task Load_UnknownUser_ReturnsEmpty()
        {
            using var db = CreateDb("pc_unknown");
            var svc = new PersistentCartService(db);

            var loaded = await svc.LoadAsync("nobody");

            Assert.Empty(loaded);
        }
    }
}
