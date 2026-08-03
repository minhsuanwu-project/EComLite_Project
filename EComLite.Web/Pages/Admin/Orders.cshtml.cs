using System;
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
using Microsoft.EntityFrameworkCore;

namespace EComLite.Web.Pages.Admin
{
    /// <summary>
    /// Admin Order Management dashboard (Version 2). Lists every customer's order
    /// and is the only UI path for advancing an order through the status lifecycle.
    /// Access is restricted to the Admin role server-side, so a non-admin cannot
    /// reach it even by typing the URL directly (mitigates R3 / privilege escalation).
    /// </summary>
    [Authorize(Roles = IdentitySeeder.AdminRole)]
    public class OrdersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly OrderStatusService _statusService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<OrdersModel> _logger;

        public OrdersModel(
            ApplicationDbContext db,
            OrderStatusService statusService,
            UserManager<IdentityUser> userManager,
            ILogger<OrdersModel> logger)
        {
            _db = db;
            _statusService = statusService;
            _userManager = userManager;
            _logger = logger;
        }

        public List<AdminOrderView> Orders { get; set; } = new();

        public class AdminOrderView
        {
            public Guid OrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public DateTime PlacedAt { get; set; }
            public decimal TotalAmount { get; set; }
            public int TotalQty { get; set; }
            public string Status { get; set; } = string.Empty;
            /// <summary>The single status this order may legally move to next, if any.</summary>
            public string? NextStatus { get; set; }
        }

        public async Task OnGetAsync() => await LoadAsync();

        public async Task<IActionResult> OnPostAdvanceAsync(Guid orderId, string newStatus)
        {
            var result = await _statusService.ChangeStatusAsync(orderId, newStatus);

            if (result.Success)
            {
                TempData["Message"] = $"Order status updated to {newStatus}.";
                _logger.LogInformation(
                    "Admin {Admin} changed order {OrderId} status to {Status}.",
                    _userManager.GetUserName(User), orderId, newStatus);
            }
            else
            {
                // Q3 (responsive): rejected transitions are surfaced and logged,
                // and the order's status is left unchanged.
                TempData["Error"] = result.Error;
                _logger.LogWarning(
                    "Admin {Admin} attempted an invalid status change on order {OrderId} to {Status}: {Reason}",
                    _userManager.GetUserName(User), orderId, newStatus, result.Error);
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            var rows = await _db.Orders
                .OrderByDescending(o => o.PlacedAt)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderNumber,
                    o.UserId,
                    o.PlacedAt,
                    o.TotalAmount,
                    o.Status,
                    TotalQty = o.Items.Sum(i => i.Qty)
                })
                .ToListAsync();

            var userIds = rows.Select(r => r.UserId).Distinct().ToList();
            var emails = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.UserName ?? u.Id);

            Orders = rows.Select(r => new AdminOrderView
            {
                OrderId = r.OrderId,
                OrderNumber = r.OrderNumber,
                CustomerEmail = emails.TryGetValue(r.UserId, out var e) ? e : r.UserId,
                PlacedAt = r.PlacedAt,
                TotalAmount = r.TotalAmount,
                TotalQty = r.TotalQty,
                Status = r.Status,
                NextStatus = OrderStatus.AllowedNext(r.Status).FirstOrDefault()
            }).ToList();
        }
    }
}
