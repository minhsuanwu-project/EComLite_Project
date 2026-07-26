using System;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EComLite.Web.Services
{
    /// <summary>
    /// Applies and validates order status changes on the server side.
    /// Enforces the lifecycle defined in <see cref="OrderStatus"/> so that an
    /// invalid transition (skipping a step or going backward) is always rejected.
    /// </summary>
    public class OrderStatusService
    {
        private readonly ApplicationDbContext _db;

        public OrderStatusService(ApplicationDbContext db)
        {
            _db = db;
        }

        public class TransitionResult
        {
            public bool Success { get; init; }
            public string? Error { get; init; }

            public static TransitionResult Ok() => new() { Success = true };
            public static TransitionResult Fail(string error) => new() { Success = false, Error = error };
        }

        /// <summary>
        /// Pure validation with no database access, so it is easy to unit test.
        /// </summary>
        public static TransitionResult ValidateTransition(string from, string to)
        {
            if (!OrderStatus.IsDefined(to))
                return TransitionResult.Fail($"'{to}' is not a valid order status.");

            if (string.Equals(from, to, StringComparison.Ordinal))
                return TransitionResult.Fail($"Order is already '{to}'.");

            if (!OrderStatus.CanTransition(from, to))
            {
                var next = OrderStatus.AllowedNext(from);
                var hint = next.Count > 0 ? string.Join(", ", next) : "none (terminal state)";
                return TransitionResult.Fail(
                    $"Cannot change status from '{from}' to '{to}'. Allowed next step: {hint}.");
            }

            return TransitionResult.Ok();
        }

        /// <summary>
        /// Loads a persisted order, validates the transition, and saves it if allowed.
        /// </summary>
        public async Task<TransitionResult> ChangeStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
                return TransitionResult.Fail("Order not found.");

            var result = ValidateTransition(order.Status, newStatus);
            if (!result.Success)
                return result;

            order.Status = newStatus;
            await _db.SaveChangesAsync();
            return result;
        }
    }
}
