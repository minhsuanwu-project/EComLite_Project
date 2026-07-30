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
        private const string CheckoutTokenKey = "CheckoutToken";

        private readonly CartService _cartService;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<IndexModel> _logger;
        private readonly CheckoutService _checkout;
        private readonly PersistentCartService _persistentCart;
        public IndexModel(
            CartService cartService,
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            ILogger<IndexModel> logger,
            CheckoutService checkout,
            PersistentCartService persistentCart)
        {
            _cartService = cartService;
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _checkout = checkout;
            _persistentCart = persistentCart;
        }

        public List<CartItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Qty * i.UnitPrice);

        public async Task OnGetAsync()
        {
            Items = _cartService.GetCart();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (Items.Count == 0)
                {
                    // Session cart is empty (e.g. the session expired). Restore the
                    // user's persisted cart so it survives session loss (UE-4.1-03).
                    var restored = await _persistentCart.LoadAsync(user.Id);
                    if (restored.Count > 0)
                    {
                        _cartService.SaveCart(restored);
                        Items = restored;
                    }
                }
                else
                {
                    // Mirror the live cart to the database so it can be restored later.
                    await _persistentCart.SaveAsync(user.Id, Items);
                }
            }

            // Issue one idempotency token per checkout attempt (UE-4.1-02).
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(CheckoutTokenKey)))
            {
                HttpContext.Session.SetString(CheckoutTokenKey, Guid.NewGuid().ToString("N"));
            }
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

            // Reuse the token issued for this checkout so a double-submit maps to
            // the same order rather than creating a duplicate (UE-4.1-02).
            var idempotencyKey = HttpContext.Session.GetString(CheckoutTokenKey)
                ?? Guid.NewGuid().ToString("N");

            try
            {
                var result = await _checkout.PlaceOrderIdempotentAsync(user.Id, Items, idempotencyKey);

                _cartService.Clear();
                await _persistentCart.ClearAsync(user.Id);
                HttpContext.Session.Remove(CheckoutTokenKey);

                _logger.LogInformation(
                    "Checkout {Outcome}. OrderId={OrderId}, OrderNumber={OrderNumber}, UserId={UserId}, Total={Total}",
                    result.Created ? "succeeded" : "was a duplicate submission",
                    result.Order.OrderId, result.Order.OrderNumber, user.Id, result.Order.TotalAmount);

                TempData["Message"] = result.Created
                    ? $"Order {result.Order.OrderNumber} placed successfully."
                    : $"Order {result.Order.OrderNumber} was already placed.";
                return RedirectToPage("/Orders/Index");
            }
            catch (Exception ex)
            {
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
        // Kept for backward compatibility; the canonical logic now lives in CheckoutService.
        internal static string GenerateOrderNumber(Guid orderId, DateTime placedAtUtc)
            => CheckoutService.GenerateOrderNumber(orderId, placedAtUtc);

    }
}

