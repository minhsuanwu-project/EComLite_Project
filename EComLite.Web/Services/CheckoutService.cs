using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EComLite.Web.Services
{
    /// <summary>
    /// Creates orders from a cart with idempotent, duplicate-safe checkout
    /// (UE-4.1-02). A checkout submission carries an idempotency key; if an order
    /// already exists for that (user, key) pair, the existing order is returned
    /// instead of creating a second one.
    /// </summary>
    public class CheckoutService
    {
        private readonly ApplicationDbContext _db;

        public CheckoutService(ApplicationDbContext db)
        {
            _db = db;
        }

        public class CheckoutResult
        {
            public Order Order { get; init; } = default!;
            /// <summary>True if a new order was created; false if an existing one was returned.</summary>
            public bool Created { get; init; }
        }

        public static string GenerateOrderNumber(Guid orderId, DateTime placedAtUtc)
        {
            var randomPart = orderId.ToString("N")[..4].ToUpper();
            return $"ORD-{placedAtUtc:yyyyMMdd}-{randomPart}";
        }

        public async Task<CheckoutResult> PlaceOrderIdempotentAsync(
            string userId, IReadOnlyList<CartItem> items, string idempotencyKey)
        {
            // Q3 (responsive): if this checkout was already processed, return the
            // existing order rather than creating a duplicate.
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await _db.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.IdempotencyKey == idempotencyKey);
                if (existing != null)
                    return new CheckoutResult { Order = existing, Created = false };
            }

            var placedAt = DateTime.UtcNow;
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = orderId,
                OrderNumber = GenerateOrderNumber(orderId, placedAt),
                UserId = userId,
                TotalAmount = items.Sum(i => i.Qty * i.UnitPrice),
                Currency = "USD",
                Status = OrderStatus.Initial,
                PlacedAt = placedAt,
                IdempotencyKey = string.IsNullOrEmpty(idempotencyKey) ? null : idempotencyKey
            };

            foreach (var item in items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Qty = item.Qty,
                    UnitPriceSnapshot = item.UnitPrice
                });
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new CheckoutResult { Order = order, Created = true };
            }
            catch (DbUpdateException)
            {
                // Q2 (preventative): a concurrent request won the race and the
                // unique index rejected this duplicate. Return the winning order.
                await transaction.RollbackAsync();
                var existing = await _db.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.IdempotencyKey == idempotencyKey);
                if (existing != null)
                    return new CheckoutResult { Order = existing, Created = false };
                throw;
            }
        }
    }
}
