using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<Auction> Auctions => Set<Auction>();

    public DbSet<Bid> Bids => Set<Bid>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<TransactionLedger> TransactionLedgers => Set<TransactionLedger>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USUARIO");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id).HasColumnName("id");

            entity.Property(user => user.Email)
                .HasColumnName("email")
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(user => user.Email).IsUnique();

            entity.Property(user => user.Name)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(user => user.RegisteredAtUtc)
                .HasColumnName("fecha_registro")
                .IsRequired();

            entity.HasOne(user => user.Wallet)
                .WithOne(wallet => wallet.User)
                .HasForeignKey<Wallet>(wallet => wallet.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(user => user.PublishedAuctions)
                .WithOne(auction => auction.Seller)
                .HasForeignKey(auction => auction.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(user => user.Bids)
                .WithOne(bid => bid.Buyer)
                .HasForeignKey(bid => bid.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(user => user.AuditLogs)
                .WithOne(auditLog => auditLog.User)
                .HasForeignKey(auditLog => auditLog.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("CATEGORIA");

            entity.HasKey(category => category.Id);

            entity.Property(category => category.Id).HasColumnName("id");

            entity.Property(category => category.Name)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(category => category.IconUrl)
                .HasColumnName("url_icono")
                .HasMaxLength(500)
                .IsRequired();

            entity.HasMany(category => category.Auctions)
                .WithOne(auction => auction.Category)
                .HasForeignKey(auction => auction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("BILLETERA");

            entity.HasKey(wallet => wallet.Id);

            entity.Property(wallet => wallet.Id).HasColumnName("id");

            entity.Property(wallet => wallet.UserId)
                .HasColumnName("usuario_id")
                .IsRequired();

            entity.HasIndex(wallet => wallet.UserId).IsUnique();

            entity.Property(wallet => wallet.TotalBalance)
                .HasColumnName("saldo_total")
                .HasPrecision(18, 2);

            entity.Property(wallet => wallet.HeldBalance)
                .HasColumnName("saldo_retenido")
                .HasPrecision(18, 2);

            entity.Property(wallet => wallet.AvailableBalance)
                .HasColumnName("saldo_disponible")
                .HasPrecision(18, 2);

            entity.Property(wallet => wallet.Version)
                .HasColumnName("version")
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Auction>(entity =>
        {
            entity.ToTable("SUBASTA");

            entity.HasKey(auction => auction.Id);

            entity.Property(auction => auction.Id).HasColumnName("id");

            entity.Property(auction => auction.SellerId)
                .HasColumnName("vendedor_id")
                .IsRequired();

            entity.Property(auction => auction.CategoryId)
                .HasColumnName("categoria_id")
                .IsRequired();

            entity.Property(auction => auction.Title)
                .HasColumnName("titulo")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(auction => auction.Description)
                .HasColumnName("descripcion")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(auction => auction.ImageUrl)
                .HasColumnName("url_imagen")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(auction => auction.BasePrice)
                .HasColumnName("precio_base")
                .HasPrecision(18, 2);

            entity.Property(auction => auction.MinimumIncrement)
                .HasColumnName("incremento_minimo")
                .HasPrecision(18, 2);

            entity.Property(auction => auction.StartAtUtc)
                .HasColumnName("fecha_inicio");

            entity.Property(auction => auction.EndAtUtc)
                .HasColumnName("fecha_fin");

            entity.Property(auction => auction.Status)
                .HasColumnName("estado")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(auction => auction.Version)
                .HasColumnName("version")
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.ToTable("PUJA");

            entity.HasKey(bid => bid.Id);

            entity.Property(bid => bid.Id).HasColumnName("id");

            entity.Property(bid => bid.AuctionId)
                .HasColumnName("subasta_id")
                .IsRequired();

            entity.Property(bid => bid.BuyerId)
                .HasColumnName("comprador_id")
                .IsRequired();

            entity.Property(bid => bid.Amount)
                .HasColumnName("monto")
                .HasPrecision(18, 2);

            entity.Property(bid => bid.BidAtUtc)
                .HasColumnName("fecha_puja");

            entity.HasOne(bid => bid.Auction)
                .WithMany(auction => auction.Bids)
                .HasForeignKey(bid => bid.AuctionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransactionLedger>(entity =>
        {
            entity.ToTable("TRANSACCION_LEDGER");

            entity.HasKey(transaction => transaction.Id);

            entity.Property(transaction => transaction.Id).HasColumnName("id");

            entity.Property(transaction => transaction.WalletId)
                .HasColumnName("billetera_id")
                .IsRequired();

            entity.Property(transaction => transaction.Type)
                .HasColumnName("tipo")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(transaction => transaction.Amount)
                .HasColumnName("monto")
                .HasPrecision(18, 2);

            entity.Property(transaction => transaction.CreatedAtUtc)
                .HasColumnName("fecha");

            entity.Property(transaction => transaction.AuctionId)
                .HasColumnName("subasta_id");

            entity.HasOne(transaction => transaction.Wallet)
                .WithMany(wallet => wallet.Transactions)
                .HasForeignKey(transaction => transaction.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(transaction => transaction.Auction)
                .WithMany(auction => auction.Transactions)
                .HasForeignKey(transaction => transaction.AuctionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AUDITORIA_LOG");

            entity.HasKey(auditLog => auditLog.Id);

            entity.Property(auditLog => auditLog.Id).HasColumnName("id");

            entity.Property(auditLog => auditLog.Entity)
                .HasColumnName("entidad")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(auditLog => auditLog.EntityId)
                .HasColumnName("entidad_id")
                .IsRequired();

            entity.Property(auditLog => auditLog.Action)
                .HasColumnName("accion")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(auditLog => auditLog.UserId)
                .HasColumnName("usuario_id");

            entity.Property(auditLog => auditLog.DetailJson)
                .HasColumnName("detalle_json")
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            entity.Property(auditLog => auditLog.CreatedAtUtc)
                .HasColumnName("fecha");
        });

        modelBuilder.Entity<Sale>()
            .Property(sale => sale.Amount)
            .HasPrecision(18, 2);
    }
}