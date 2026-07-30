using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EComLite.Web.Services
{
    /// <summary>
    /// Persists an authenticated user's cart to the database, keyed by user ID,
    /// so the cart survives session expiry during checkout (UE-4.1-03).
    /// </summary>
    public class PersistentCartService
    {
        private readonly ApplicationDbContext _db;

        public PersistentCartService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<CartItem>> LoadAsync(string userId)
        {
            var row = await _db.PersistedCarts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (row == null || string.IsNullOrEmpty(row.ItemsJson))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(row.ItemsJson) ?? new List<CartItem>();
        }

        public async Task SaveAsync(string userId, List<CartItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            var row = await _db.PersistedCarts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (row == null)
            {
                _db.PersistedCarts.Add(new PersistedCart
                {
                    UserId = userId,
                    ItemsJson = json,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                row.ItemsJson = json;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }

        public async Task ClearAsync(string userId)
        {
            var row = await _db.PersistedCarts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (row != null)
            {
                _db.PersistedCarts.Remove(row);
                await _db.SaveChangesAsync();
            }
        }
    }
}
