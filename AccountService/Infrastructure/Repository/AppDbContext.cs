using AccountService.Features.Account;
using AccountService.Features.Outbox;
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
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxConsumed> InboxConsumed => Set<InboxConsumed>();
    public DbSet<InboxDeadLetter> InboxDeadLetters => Set<InboxDeadLetter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Version)
                .IsRowVersion()
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(a => a.OwnerId)
                .HasMethod("hash");

            entity.Property(e => e.CloseDate)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.OpenDate)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.Balance)
                .HasColumnType("numeric(18,2)");

            entity.Property(e => e.PercentageRate)
                .HasColumnType("numeric(5,2)");
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

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.EventId).HasDefaultValueSql("gen_random_uuid()").IsRequired();
            entity.Property(o => o.AggregateType).IsRequired();
            entity.Property(o => o.AggregateId).IsRequired();
            entity.Property(o => o.Type).IsRequired();
            entity.Property(o => o.Payload).IsRequired();
            entity.Property(o => o.OccurredAt).IsRequired();
            entity.Property(o => o.ProcessedAt);
            entity.Property(o => o.CorrelationId);
            entity.Property(o => o.CausationId);
            entity.Property(o => o.RetryCount).HasDefaultValue(0);
            entity.Property(o => o.LastError);
            entity.Property(o => o.PublishedLatencyMs);
        });

        modelBuilder.Entity<InboxConsumed>(entity =>
        {
            entity.ToTable("inbox_consumed");
            entity.HasKey(i => i.MessageId);
            entity.Property(i => i.Handler).IsRequired();
            entity.Property(i => i.ProcessedAt).IsRequired();
            entity.HasKey(x => new { x.MessageId, x.Handler });
        });

        modelBuilder.Entity<InboxDeadLetter>(entity =>
        {
            entity.ToTable("inbox_dead_letters");
            entity.HasKey(d => d.MessageId);
            entity.Property(d => d.Handler).IsRequired();
            entity.Property(d => d.ReceivedAt).IsRequired();
            entity.Property(d => d.Payload).IsRequired();
            entity.Property(d => d.Error).IsRequired();
        });
    }
}