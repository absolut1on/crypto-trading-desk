using Gateway.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderEvent> OrderEvents => Set<OrderEvent>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletEvent> WalletEvents => Set<WalletEvent>();
    public DbSet<CryptoPair> CryptoPairs => Set<CryptoPair>();
    public DbSet<Saga> Sagas => Set<Saga>();
    public DbSet<SagaStep> SagaSteps => Set<SagaStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.SupabaseId).IsUnique();
            e.Property(u => u.Status).HasConversion<string>();
        });

        modelBuilder.Entity<UserEvent>(e =>
        {
            e.HasIndex(ue => ue.UserId);
            e.HasIndex(ue => ue.Timestamp);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.UserId);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.Pair);
            e.Property(o => o.Status).HasConversion<string>();
            e.Property(o => o.Side).HasConversion<string>();
            e.Property(o => o.Type).HasConversion<string>();
            e.Property(o => o.Quantity).HasPrecision(18, 8);
            e.Property(o => o.Price).HasPrecision(18, 8);
        });

        modelBuilder.Entity<OrderEvent>(e =>
        {
            e.HasIndex(oe => oe.OrderId);
            e.HasIndex(oe => oe.Timestamp);
        });

        modelBuilder.Entity<Trade>(e =>
        {
            e.HasIndex(t => t.UserId);
            e.HasIndex(t => t.OrderId);
            e.HasIndex(t => t.Pair);
            e.Property(t => t.Side).HasConversion<string>();
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Quantity).HasPrecision(18, 8);
            e.Property(t => t.Price).HasPrecision(18, 8);
            e.Property(t => t.Fee).HasPrecision(18, 8);
        });

        modelBuilder.Entity<Wallet>(e =>
        {
            e.HasIndex(w => new { w.UserId, w.Currency }).IsUnique();
        });

        modelBuilder.Entity<WalletEvent>(e =>
        {
            e.HasIndex(we => we.WalletId);
            e.HasIndex(we => we.Timestamp);
            e.Property(we => we.EventType).HasConversion<string>();
            e.Property(we => we.Amount).HasPrecision(18, 8);
            e.Property(we => we.BalanceAfter).HasPrecision(18, 8);
        });

        modelBuilder.Entity<CryptoPair>(e =>
        {
            e.HasIndex(cp => cp.Symbol).IsUnique();
        });

        modelBuilder.Entity<Saga>(e =>
        {
            e.Property(s => s.State).HasConversion<string>();
        });

        modelBuilder.Entity<SagaStep>(e =>
        {
            e.HasIndex(ss => ss.SagaId);
            e.Property(ss => ss.Status).HasConversion<string>();
        });
    }
}
