using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleOrderSystem.Entities;
using SimpleOrderSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SimpleOrderSystem.Data
{
    public class OrderDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;

        public OrderDbContext(DbContextOptions<OrderDbContext> options, ICurrentUser currentUser)
            : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<GroupClaim> GroupClaims { get; set; }
        public DbSet<OperationClaim> OperationClaims { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Order> Orders { get; set; }

        internal bool IsSeeding { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureBase(modelBuilder.Entity<User>(), "Users");
            ConfigureBase(modelBuilder.Entity<Group>(), "Groups");
            ConfigureBase(modelBuilder.Entity<UserGroup>(), "UserGroups");
            ConfigureBase(modelBuilder.Entity<GroupClaim>(), "GroupClaims");
            ConfigureBase(modelBuilder.Entity<OperationClaim>(), "OperationClaims");
            ConfigureBase(modelBuilder.Entity<Customer>(), "Customers");
            ConfigureBase(modelBuilder.Entity<Color>(), "Colors");
            ConfigureBase(modelBuilder.Entity<Product>(), "Products");
            ConfigureBase(modelBuilder.Entity<Stock>(), "Stocks");
            ConfigureBase(modelBuilder.Entity<Order>(), "Orders");

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Gender).HasMaxLength(10).IsRequired();
                entity.Property(x => x.PasswordHash).HasMaxLength(32).IsRequired();
                entity.Property(x => x.PasswordSalt).HasMaxLength(32).IsRequired();
                entity.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.ToTable(table => table.HasCheckConstraint("CK_Users_Gender", "[Gender] IN ('Male', 'Female', 'Other')"));
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(x => x.GroupName).HasMaxLength(60).IsRequired();
                entity.HasIndex(x => x.GroupName).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Group>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.UserId, x.GroupId }).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<OperationClaim>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
                entity.Property(x => x.Alias).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<GroupClaim>(entity =>
            {
                entity.HasOne<Group>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<OperationClaim>().WithMany().HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.GroupId, x.ClaimId }).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Address).HasMaxLength(500).IsRequired();
                entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
                entity.HasIndex(x => x.Code).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<Color>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Size).HasConversion<int>();
                entity.HasOne<Color>().WithMany().HasForeignKey(x => x.ColorId).OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table => table.HasCheckConstraint("CK_Products_Size", "[Size] IN (1, 2, 3, 4)"));
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.Property(x => x.RowVersion).IsRowVersion();
                entity.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => x.ProductId).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.ToTable(table => table.HasCheckConstraint("CK_Stocks_Quantity", "[Quantity] >= 0"));
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table => table.HasCheckConstraint("CK_Orders_Quantity", "[Quantity] > 0"));
            });
        }

        private static void ConfigureBase<T>(EntityTypeBuilder<T> entity, string tableName) where T : BaseEntity
        {
            entity.HasBaseType((Type)null);
            entity.ToTable(tableName);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.CreatedUserId).IsRequired();
            entity.Property(x => x.CreatedDate).HasColumnType("datetime2").IsRequired();
            entity.Property(x => x.LastUpdatedUserId).IsRequired();
            entity.Property(x => x.LastUpdatedDate).HasColumnType("datetime2").IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetAuditFields();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetAuditFields()
        {
            var now = DateTime.UtcNow;
            var userId = IsSeeding ? 1 : _currentUser.UserId;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified && entry.State != EntityState.Deleted)
                {
                    continue;
                }

                if (userId <= 0)
                {
                    throw new InvalidOperationException("A signed-in user is required to save changes.");
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedUserId = userId;
                    entry.Entity.CreatedDate = now;
                    entry.Entity.IsDeleted = false;
                }
                else
                {
                    if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                    }

                    entry.Property(x => x.CreatedUserId).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;
                }

                entry.Entity.LastUpdatedUserId = userId;
                entry.Entity.LastUpdatedDate = now;
            }
        }
    }
}
