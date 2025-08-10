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

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.OwnerId)
            .HasMethod("hash");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.AccountId, t.Date });

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.Date)
            .HasMethod("gist");

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

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
                .HasColumnType("timestamp without time zone");

            entity.HasOne<Account>()
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}