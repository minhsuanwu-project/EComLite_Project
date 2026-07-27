using EComLite.Web.Models;
using EComLite.Web.Services;
using Xunit;

namespace EComLite.Tests
{
    // Tests for Version 2 Item 4: server-side order status transition validation.
    public class OrderStatusTransitionTests
    {
        [Theory]
        [InlineData("Pending", "Processing")]
        [InlineData("Processing", "Shipped")]
        [InlineData("Shipped", "Delivered")]
        public void ValidForwardTransition_IsAllowed(string from, string to)
        {
            var result = OrderStatusService.ValidateTransition(from, to);
            Assert.True(result.Success, result.Error);
        }

        [Theory]
        [InlineData("Pending", "Shipped")]      // skips Processing
        [InlineData("Pending", "Delivered")]    // skips two steps
        [InlineData("Processing", "Delivered")] // skips Shipped
        [InlineData("Shipped", "Processing")]   // backward
        [InlineData("Delivered", "Shipped")]    // backward from terminal
        [InlineData("Processing", "Pending")]   // backward
        public void InvalidTransition_IsRejected(string from, string to)
        {
            var result = OrderStatusService.ValidateTransition(from, to);
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public void SameStatus_IsRejected()
        {
            var result = OrderStatusService.ValidateTransition("Pending", "Pending");
            Assert.False(result.Success);
        }

        [Fact]
        public void UnknownTargetStatus_IsRejected()
        {
            var result = OrderStatusService.ValidateTransition("Pending", "Frozen");
            Assert.False(result.Success);
        }

        [Fact]
        public void DeliveredIsTerminal()
        {
            Assert.Empty(OrderStatus.AllowedNext(OrderStatus.Delivered));
        }

        [Fact]
        public void NewOrdersStartPending()
        {
            Assert.Equal(OrderStatus.Pending, OrderStatus.Initial);
        }
    }
}
