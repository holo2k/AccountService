using AccountService.Features.Account;
using AccountService.Features.Transaction;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property<uint>("Version")
                .IsRowVersion()
                .HasColumnName("xmin");

            entity.HasIndex(a => a.OwnerId)
                .HasMethod("hash");

            entity.Property(e => e.CloseDate)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.OpenDate)
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasIndex(t => new { t.AccountId, t.Date });

            entity.HasIndex(t => t.Date)
                .HasMethod("gist");

            entity.Property(t => t.Type)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(t => t.Description)
                .HasMaxLength(500);

            entity.Property(t => t.Date)
                .HasColumnType("timestamp with time zone");

            entity.HasOne<Account>()
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}