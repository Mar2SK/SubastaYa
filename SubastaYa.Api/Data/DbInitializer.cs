using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            await RefreshDemoAuctionsAsync(context);
            return;
        }

        DateTime nowUtc = DateTime.UtcNow;

        Category technology = new()
        {
            Name = "Tecnología",
            IconUrl = "https://placehold.co/100x100?text=Tecnologia"
        };

        Category collectibles = new()
        {
            Name = "Coleccionables",
            IconUrl = "https://placehold.co/100x100?text=Coleccionables"
        };

        Category clothing = new()
        {
            Name = "Indumentaria",
            IconUrl = "https://placehold.co/100x100?text=Indumentaria"
        };

        Category vehicles = new()
        {
            Name = "Vehículos",
            IconUrl = "https://placehold.co/100x100?text=Vehiculos"
        };

        User seller = new()
        {
            Email = "vendedor@test.com",
            Name = "Vendedor",
            PasswordHash = "123",
            RegisteredAtUtc = nowUtc
        };

        User buyer1 = new()
        {
            Email = "comprador1@test.com",
            Name = "Comprador Uno",
            PasswordHash = "123",
            RegisteredAtUtc = nowUtc
        };

        User buyer2 = new()
        {
            Email = "comprador2@test.com",
            Name = "Comprador Dos",
            PasswordHash = "123",
            RegisteredAtUtc = nowUtc
        };

        User noFundsBuyer = new()
        {
            Email = "sinfondos@test.com",
            Name = "Comprador Sin Fondos",
            PasswordHash = "123",
            RegisteredAtUtc = nowUtc
        };

        Wallet sellerWallet = new()
        {
            User = seller,
            TotalBalance = 0,
            HeldBalance = 0,
            AvailableBalance = 0,
            Version = 1
        };

        Wallet buyer1Wallet = new()
        {
            User = buyer1,
            TotalBalance = 150000,
            HeldBalance = 45000,
            AvailableBalance = 105000,
            Version = 1
        };

        Wallet buyer2Wallet = new()
        {
            User = buyer2,
            TotalBalance = 200000,
            HeldBalance = 0,
            AvailableBalance = 200000,
            Version = 1
        };

        Wallet noFundsWallet = new()
        {
            User = noFundsBuyer,
            TotalBalance = 500,
            HeldBalance = 0,
            AvailableBalance = 500,
            Version = 1
        };

        seller.Wallet = sellerWallet;
        buyer1.Wallet = buyer1Wallet;
        buyer2.Wallet = buyer2Wallet;
        noFundsBuyer.Wallet = noFundsWallet;

        context.Categories.AddRange(
            technology,
            collectibles,
            clothing,
            vehicles);

        context.Users.AddRange(
            seller,
            buyer1,
            buyer2,
            noFundsBuyer);

        await context.SaveChangesAsync();

        Auction standardActiveAuction = new()
        {
            Seller = seller,
            Category = technology,
            Title = "Notebook Gamer",
            Description = "Notebook para pruebas de pujas en tiempo real.",
            ImageUrl = "https://placehold.co/600x400?text=Notebook+Gamer",
            BasePrice = 30000,
            MinimumIncrement = 5000,
            StartAtUtc = nowUtc.AddMinutes(-30),
            EndAtUtc = nowUtc.AddMinutes(25),
            Status = "ACTIVA",
            Version = 1
        };

        Auction criticalActiveAuction = new()
        {
            Seller = seller,
            Category = collectibles,
            Title = "Figura de colección",
            Description = "Subasta próxima a finalizar para probar anti-sniping.",
            ImageUrl = "https://placehold.co/600x400?text=Figura",
            BasePrice = 10000,
            MinimumIncrement = 1000,
            StartAtUtc = nowUtc.AddMinutes(-20),
            EndAtUtc = nowUtc.AddSeconds(90),
            Status = "ACTIVA",
            Version = 1
        };

        Auction upcomingAuction = new()
        {
            Seller = seller,
            Category = clothing,
            Title = "Campera de cuero",
            Description = "Subasta programada para iniciar dentro de 24 horas.",
            ImageUrl = "https://placehold.co/600x400?text=Campera",
            BasePrice = 20000,
            MinimumIncrement = 2000,
            StartAtUtc = nowUtc.AddHours(24),
            EndAtUtc = nowUtc.AddHours(25),
            Status = "PROGRAMADA",
            Version = 1
        };

        Auction expiredWithWinnerAuction = new()
        {
            Seller = seller,
            Category = vehicles,
            Title = "Bicicleta urbana",
            Description = "Subasta vencida con una puja ganadora para probar el Worker.",
            ImageUrl = "https://placehold.co/600x400?text=Bicicleta",
            BasePrice = 50000,
            MinimumIncrement = 5000,
            StartAtUtc = nowUtc.AddDays(-2),
            EndAtUtc = nowUtc.AddMinutes(-5),
            Status = "ACTIVA",
            Version = 1
        };

        Auction expiredDesertedAuction = new()
        {
            Seller = seller,
            Category = technology,
            Title = "Monitor usado",
            Description = "Subasta vencida sin pujas para probar estado DESIERTA.",
            ImageUrl = "https://placehold.co/600x400?text=Monitor",
            BasePrice = 25000,
            MinimumIncrement = 2500,
            StartAtUtc = nowUtc.AddDays(-2),
            EndAtUtc = nowUtc.AddMinutes(-10),
            Status = "ACTIVA",
            Version = 1
        };

        context.Auctions.AddRange(
            standardActiveAuction,
            criticalActiveAuction,
            upcomingAuction,
            expiredWithWinnerAuction,
            expiredDesertedAuction);

        await context.SaveChangesAsync();

        Bid previousBid = new()
        {
            Auction = standardActiveAuction,
            Buyer = buyer2,
            Amount = 40000,
            BidAtUtc = nowUtc.AddMinutes(-15)
        };

        Bid winningBid = new()
        {
            Auction = standardActiveAuction,
            Buyer = buyer1,
            Amount = 45000,
            BidAtUtc = nowUtc.AddMinutes(-10)
        };

        Bid expiredWinnerBid = new()
        {
            Auction = expiredWithWinnerAuction,
            Buyer = buyer2,
            Amount = 60000,
            BidAtUtc = nowUtc.AddHours(-1)
        };

        context.Bids.AddRange(
            previousBid,
            winningBid,
            expiredWinnerBid);

        context.TransactionLedgers.AddRange(
            new TransactionLedger
            {
                Wallet = buyer1Wallet,
                Auction = standardActiveAuction,
                Type = "DEPOSITO",
                Amount = 150000,
                CreatedAtUtc = nowUtc.AddDays(-1)
            },
            new TransactionLedger
            {
                Wallet = buyer1Wallet,
                Auction = standardActiveAuction,
                Type = "RETENCION",
                Amount = 45000,
                CreatedAtUtc = nowUtc.AddMinutes(-10)
            },
            new TransactionLedger
            {
                Wallet = buyer2Wallet,
                Type = "DEPOSITO",
                Amount = 260000,
                CreatedAtUtc = nowUtc.AddDays(-1)
            },
            new TransactionLedger
            {
                Wallet = noFundsWallet,
                Type = "DEPOSITO",
                Amount = 200000,
                CreatedAtUtc = nowUtc.AddDays(-1)
            },
            new TransactionLedger
            {
                Wallet = buyer2Wallet,
                Auction = expiredWithWinnerAuction,
                Type = "RETENCION",
                Amount = 60000,
                CreatedAtUtc = nowUtc.AddHours(-1)
            });

        context.AuditLogs.AddRange(
            new AuditLog
            {
                Entity = "BILLETERA",
                EntityId = buyer1Wallet.Id,
                Action = "ACREDITACION_MANUAL",
                User = buyer1,
                DetailJson = "{\"monto\":150000}",
                CreatedAtUtc = nowUtc.AddDays(-1)
            },
            new AuditLog
            {
                Entity = "SUBASTA",
                EntityId = standardActiveAuction.Id,
                Action = "SUBASTA_CREADA",
                User = seller,
                DetailJson = "{\"titulo\":\"Notebook Gamer\"}",
                CreatedAtUtc = nowUtc.AddMinutes(-30)
            });

        await context.SaveChangesAsync();
    }

    private static async Task RefreshDemoAuctionsAsync(AppDbContext context)
    {
        DateTime nowUtc = DateTime.UtcNow;

        Auction? standardAuction =
            await context.Auctions
                .FirstOrDefaultAsync(auction =>
                    auction.Title == "Notebook Gamer");

        if (standardAuction is not null &&
            standardAuction.Status == "FINALIZADA")
        {
            standardAuction.Status = "ACTIVA";
            standardAuction.EndAtUtc = nowUtc.AddMinutes(25);
            standardAuction.Version += 1;
        }

        Auction? criticalAuction =
            await context.Auctions
                .FirstOrDefaultAsync(auction =>
                    auction.Title == "Figura de colección");

        if (criticalAuction is not null &&
            criticalAuction.Status == "FINALIZADA")
        {
            criticalAuction.Status = "ACTIVA";
            criticalAuction.EndAtUtc = nowUtc.AddMinutes(2);
            criticalAuction.Version += 1;
        }

        await context.SaveChangesAsync();
    }
}