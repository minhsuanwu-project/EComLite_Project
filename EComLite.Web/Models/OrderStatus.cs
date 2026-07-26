using System;
using System.Collections.Generic;

namespace EComLite.Web.Models
{
    /// <summary>
    /// Canonical order status lifecycle for Version 2:
    /// Pending -> Processing -> Shipped -> Delivered.
    /// Statuses are stored as strings on <see cref="Order.Status"/>.
    /// </summary>
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";

        /// <summary>The status a newly placed order starts in.</summary>
        public const string Initial = Pending;

        /// <summary>The lifecycle in order.</summary>
        public static readonly IReadOnlyList<string> Lifecycle = new[]
        {
            Pending, Processing, Shipped, Delivered
        };

        // Allowed forward transitions: exactly one step, no skipping, no going back.
        private static readonly IReadOnlyDictionary<string, string[]> Allowed =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [Pending]    = new[] { Processing },
                [Processing] = new[] { Shipped },
                [Shipped]    = new[] { Delivered },
                [Delivered]  = Array.Empty<string>(), // terminal state
            };

        /// <summary>True if the value is a known lifecycle status.</summary>
        public static bool IsDefined(string status)
            => status != null && Allowed.ContainsKey(status);

        /// <summary>The statuses that may legally follow <paramref name="from"/>.</summary>
        public static IReadOnlyList<string> AllowedNext(string from)
            => Allowed.TryGetValue(from ?? string.Empty, out var next) ? next : Array.Empty<string>();

        /// <summary>True if moving <paramref name="from"/> -> <paramref name="to"/> is a legal single step.</summary>
        public static bool CanTransition(string from, string to)
            => IsDefined(from) && IsDefined(to) && Array.IndexOf(Allowed[from], to) >= 0;
    }
}
