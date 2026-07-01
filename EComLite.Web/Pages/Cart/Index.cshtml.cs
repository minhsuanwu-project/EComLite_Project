using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EComLite.Web.Data;
using EComLite.Web.Models;
using EComLite.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EComLite.Web.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(
            CartService cartService,
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            ILogger<IndexModel> logger)
        {
            _cartService = cartService;
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public List<CartItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Qty * i.UnitPrice);

        public void OnGet()
        {
            Items = _cartService.GetCart();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            Items = _cartService.GetCart();
            if (!Items.Any())
            {
                TempData["Message"] = "Cart is empty.";
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var placedAt = DateTime.UtcNow;
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                OrderNumber = GenerateOrderNumber(orderId, placedAt),
                UserId = user.Id,
                TotalAmount = Total,
                Currency = "USD",
                Status = "Placed",
                PlacedAt = placedAt
            };

            foreach (var item in Items)
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
                //throw new Exception("Simulating a database write failure midway！");
                await transaction.CommitAsync();

                _cartService.Clear();

                _logger.LogInformation(
                    "Checkout succeeded. OrderId={OrderId}, OrderNumber={OrderNumber}, UserId={UserId}, Total={Total}, ItemCount={ItemCount}",
                    order.OrderId, order.OrderNumber, user.Id, order.TotalAmount, Items.Count);

                TempData["Message"] = $"Order {order.OrderNumber} placed successfully.";
                return RedirectToPage("/Orders/Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex,
                    "Checkout failed. UserId={UserId}, ItemCount={ItemCount}, AttemptedTotal={Total}",
                    user.Id, Items.Count, Total);

                TempData["Message"] = "An error occurred while placing your order. Please try again.";
                return RedirectToPage();
            }           
        }
        public IActionResult OnPostClear()
        {
            _cartService.Clear();
            return RedirectToPage();
        }
        public IActionResult OnPostRemove(Guid productId)
        {
            _cartService.Remove(productId);
            return RedirectToPage();
        }
        internal static string GenerateOrderNumber(Guid orderId, DateTime placedAtUtc)
        {            
            var randomPart = orderId.ToString("N")[..4].ToUpper(); // Get first 4-digit from OrderId 
            return $"ORD-{placedAtUtc:yyyyMMdd}-{randomPart}";
        }

    }
}

