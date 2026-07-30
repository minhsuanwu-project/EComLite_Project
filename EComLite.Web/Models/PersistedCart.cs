using System;

namespace EComLite.Web.Models
{
    /// <summary>
    /// A shopping cart persisted to the database and keyed by user ID, so an
    /// authenticated user's cart survives session expiry (UE-4.1-03). The cart
    /// items are stored as serialized JSON.
    /// </summary>
    public class PersistedCart
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string ItemsJson { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
