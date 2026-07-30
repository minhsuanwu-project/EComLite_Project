using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EComLite.Web.Models;

namespace EComLite.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderItem> OrderItems { get; set; } = default!;
    public DbSet<PersistedCart> PersistedCarts { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // One persisted cart per user (UE-4.1-03).
        builder.Entity<PersistedCart>()
            .HasIndex(c => c.UserId)
            .IsUnique();

        // Filtered unique index: an idempotency key, when present, may back at
        // most one order, blocking duplicate/concurrent checkouts (UE-4.1-02).
        builder.Entity<Order>()
            .HasIndex(o => o.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}
