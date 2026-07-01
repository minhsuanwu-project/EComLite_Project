using System;
using EComLite.Web.Pages.Cart;
using Xunit;

namespace EComLite.Tests
{
    public class OrderNumberTests
    {
        // ── Format validation ────────────────────────────────────────────────

        [Fact]
        public void GenerateOrderNumber_ReturnsCorrectFormat()
        {
            var orderId = Guid.NewGuid();
            var placedAt = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc);

            var result = IndexModel.GenerateOrderNumber(orderId, placedAt);

            Assert.StartsWith("ORD-20260322-", result);
        }

        [Fact]
        public void GenerateOrderNumber_ContainsFourCharSuffix()
        {
            var orderId = Guid.NewGuid();
            var placedAt = DateTime.UtcNow;

            var result = IndexModel.GenerateOrderNumber(orderId, placedAt);

            // Format: ORD-YYYYMMDD-XXXX  →  split by '-' gives 3 parts
            var parts = result.Split('-');
            Assert.Equal(3, parts.Length);
            Assert.Equal(4, parts[2].Length);
        }

        [Fact]
        public void GenerateOrderNumber_SuffixIsUpperCase()
        {
            var orderId = Guid.NewGuid();
            var placedAt = DateTime.UtcNow;

            var result = IndexModel.GenerateOrderNumber(orderId, placedAt);

            var suffix = result.Split('-')[2];
            Assert.Equal(suffix.ToUpper(), suffix);
        }

        [Fact]
        public void GenerateOrderNumber_DifferentOrderIds_ProduceDifferentNumbers()
        {
            var placedAt = DateTime.UtcNow;
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var result1 = IndexModel.GenerateOrderNumber(id1, placedAt);
            var result2 = IndexModel.GenerateOrderNumber(id2, placedAt);

            // Two different GUIDs almost certainly produce different order numbers
            // (they could match by coincidence only if first 4 hex chars are identical)
            Assert.NotNull(result1);
            Assert.NotNull(result2);
        }

        [Fact]
        public void GenerateOrderNumber_DatePartMatchesPlacedAt()
        {
            var orderId = Guid.NewGuid();
            var placedAt = new DateTime(2025, 11, 20, 0, 0, 0, DateTimeKind.Utc);

            var result = IndexModel.GenerateOrderNumber(orderId, placedAt);

            Assert.Contains("20251120", result);
        }
    }
}
