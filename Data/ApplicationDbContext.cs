using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CoffeePartner> CoffeePartners { get; set; }
        public DbSet<CoffeeGroundBatch> CoffeeGroundBatches { get; set; }
        public DbSet<ProductionBatch> ProductionBatches { get; set; }
        public DbSet<ProductStory> ProductStories { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }
        public DbSet<QRScanLog> QRScanLogs { get; set; }
        public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RewardRedemption> RewardRedemptions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductCustomization> ProductCustomizations { get; set; }
        public DbSet<B2BRequest> B2BRequests { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Level).HasDefaultValue(CustomerLevel.Normal);
            });

            // Address Configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReceiverName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Province).IsRequired().HasMaxLength(100);
                entity.Property(e => e.District).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ward).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DetailAddress).IsRequired().HasMaxLength(255);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.SalePrice).HasPrecision(18, 2);
                entity.Property(e => e.ShortDescription).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Material).HasMaxLength(200);
                entity.Property(e => e.Size).HasMaxLength(100);
                entity.Property(e => e.UsageNote).HasMaxLength(500);
                entity.Property(e => e.Model3DUrl).HasMaxLength(500);
                entity.Property(e => e.Model3DPublicId).HasMaxLength(255);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductImage Configuration
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductVariant Configuration
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VariantName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Size).HasMaxLength(50);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InventoryTransaction Configuration
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Note).HasMaxLength(500);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.InventoryTransactions)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Variant)
                    .WithMany()
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cart Configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Cart)
                    .HasForeignKey<Cart>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem Configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PersonalizationNote).HasMaxLength(500);

                entity.HasOne(e => e.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Variant)
                    .WithMany()
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.ShippingFee).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.Note).HasMaxLength(500);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ShippingAddress)
                    .WithMany()
                    .HasForeignKey(e => e.ShippingAddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Coupon)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(e => e.CouponId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem Configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
                entity.Property(e => e.PersonalizationNote).HasMaxLength(500);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Variant)
                    .WithMany()
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.TransactionCode).HasMaxLength(100);

                entity.HasOne(e => e.Order)
                    .WithOne(o => o.Payment)
                    .HasForeignKey<Payment>(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Shipment Configuration
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CarrierName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TrackingCode).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Order)
                    .WithOne(o => o.Shipment)
                    .HasForeignKey<Shipment>(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Coupon Configuration
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Value).HasPrecision(18, 2);
                entity.Property(e => e.MinimumOrderAmount).HasPrecision(18, 2);
            });



            // CoffeePartner Configuration
            modelBuilder.Entity<CoffeePartner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContactName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            });

            // CoffeeGroundBatch Configuration
            modelBuilder.Entity<CoffeeGroundBatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WeightKg).HasPrecision(18, 2);
                entity.Property(e => e.ProcessingStatus).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Note).HasMaxLength(500);

                entity.HasOne(e => e.Partner)
                    .WithMany(p => p.CoffeeGroundBatches)
                    .HasForeignKey(e => e.PartnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductionBatch Configuration
            modelBuilder.Entity<ProductionBatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BatchCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.QualityStatus).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.CoffeeGroundBatch)
                    .WithMany(b => b.ProductionBatches)
                    .HasForeignKey(e => e.CoffeeGroundBatchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductStory Configuration
            modelBuilder.Entity<ProductStory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginStory).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.RecyclingProcess).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.SustainabilityMessage).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.EstimatedWasteReducedGram).HasPrecision(18, 2);

                entity.HasOne(e => e.Product)
                    .WithOne(p => p.ProductStory)
                    .HasForeignKey<ProductStory>(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductionBatch)
                    .WithMany(b => b.ProductStories)
                    .HasForeignKey(e => e.ProductionBatchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // QRCode Configuration
            modelBuilder.Entity<QRCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QRValue).IsRequired().HasMaxLength(250);
                entity.Property(e => e.LandingPageUrl).IsRequired().HasMaxLength(500);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.QRCodes)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductStory)
                    .WithMany(s => s.QRCodes)
                    .HasForeignKey(e => e.ProductStoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // QRScanLog Configuration
            modelBuilder.Entity<QRScanLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.DeviceInfo).HasMaxLength(250);

                entity.HasOne(e => e.QRCode)
                    .WithMany(q => q.QRScanLogs)
                    .HasForeignKey(e => e.QRCodeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LoyaltyPointTransaction Configuration
            modelBuilder.Entity<LoyaltyPointTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(250);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.LoyaltyPointTransactions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.QRScanLog)
                    .WithMany()
                    .HasForeignKey(e => e.QRScanLogId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Reward Configuration
            modelBuilder.Entity<Reward>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            });

            // RewardRedemption Configuration
            modelBuilder.Entity<RewardRedemption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RedemptionCode).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RewardRedemptions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reward)
                    .WithMany(r => r.RewardRedemptions)
                    .HasForeignKey(e => e.RewardId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Review Configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comment).HasMaxLength(1000);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductCustomization Configuration
            modelBuilder.Entity<ProductCustomization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SourceImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SourceImagePublicId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PreviewImageUrl).HasMaxLength(500);
                entity.Property(e => e.ResultModelUrl).HasMaxLength(500);
                entity.Property(e => e.ResultModelPublicId).HasMaxLength(255);
                entity.Property(e => e.Note).HasMaxLength(1000);
                entity.Property(e => e.FailureReason).HasMaxLength(1000);

                entity.Property(e => e.PositionX).HasPrecision(8, 3);
                entity.Property(e => e.PositionY).HasPrecision(8, 3);
                entity.Property(e => e.PositionZ).HasPrecision(8, 3);
                entity.Property(e => e.RotationX).HasPrecision(8, 3);
                entity.Property(e => e.RotationY).HasPrecision(8, 3);
                entity.Property(e => e.RotationZ).HasPrecision(8, 3);
                entity.Property(e => e.Scale).HasPrecision(8, 3);
                entity.Property(e => e.EngraveDepth).HasPrecision(8, 3);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ProductCustomizations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.ProductCustomizations)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // B2BRequest Configuration
            modelBuilder.Entity<B2BRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.ContactName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ProductRequirement).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ExpectedBudget).HasPrecision(18, 2);
                entity.Property(e => e.AdminNote).HasMaxLength(1000);
            });

            // BlogPost Configuration
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
                entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
