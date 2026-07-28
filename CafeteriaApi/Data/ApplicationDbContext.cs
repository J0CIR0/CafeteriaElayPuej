using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Models;

namespace CafeteriaApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<QRPayment> QRPayments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<IngredientMovement> IngredientMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Tax)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Subtotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<QRPayment>()
                .Property(q => q.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.StockQuantity)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.MinStockQuantity)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.UnitCost)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ProductIngredient>()
                .Property(pi => pi.QuantityRequired)
                .HasPrecision(10, 2);

            modelBuilder.Entity<IngredientMovement>()
                .Property(im => im.Quantity)
                .HasPrecision(10, 2);

            modelBuilder.Entity<IngredientMovement>()
                .Property(im => im.UnitCostAtTime)
                .HasPrecision(10, 2);

            modelBuilder.Entity<IngredientMovement>()
                .Property(im => im.TotalCostLoss)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductIngredients)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Ingredient)
                .WithMany(i => i.ProductIngredients)
                .HasForeignKey(pi => pi.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IngredientMovement>()
                .HasOne(im => im.Ingredient)
                .WithMany(i => i.IngredientMovements)
                .HasForeignKey(im => im.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmailVerification>()
                .HasIndex(e => e.Code);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(e => e.Code);

            modelBuilder.Entity<User>()
                .Property(u => u.IsEmailVerified)
                .HasDefaultValue(false);
        }
    }
}