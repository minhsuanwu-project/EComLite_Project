using System;
using System.Collections.Generic;

namespace EComLite.Web.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Currency { get; set; } = "USD";

        public string Status { get; set; } = OrderStatus.Initial;

        /// <summary>
        /// Idempotency token for the checkout submission that created this order.
        /// A filtered unique index prevents two orders from sharing the same key,
        /// which stops duplicate/concurrent checkout submissions (UE-4.1-02).
        /// </summary>
        public string? IdempotencyKey { get; set; }

        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

