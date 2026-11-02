
using Microsoft.AspNetCore.Http;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public class BidBusinessRules
{
    public void ValidateAuction(
        Auction auction,
        int buyerId,
        DateTime nowUtc)
    {
        if (auction.SellerId == buyerId)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el vendedor no puede pujar en su propia subasta.");
        }

        if (auction.Status == "PROGRAMADA" ||
            auction.StartAtUtc > nowUtc)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "la subasta todavía no comenzó.");
        }

        if (auction.EndAtUtc <= nowUtc)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "la subasta ya finalizó.");
        }

        if (auction.Status != "ACTIVA")
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "la subasta no se encuentra activa.");
        }
    }

    public decimal GetMinimumRequiredAmount(
        Auction auction,
        Bid? previousLeadingBid)
    {
        return previousLeadingBid is null
            ? auction.BasePrice
            : previousLeadingBid.Amount + auction.MinimumIncrement;
    }

    public void ValidateAmount(
        decimal amount,
        decimal minimumRequiredAmount)
    {
        if (amount < minimumRequiredAmount)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"la puja mínima permitida es {minimumRequiredAmount}.");
        }
    }

    public void ValidateAvailableBalance(
        Wallet buyerWallet,
        Bid? previousLeadingBid,
        int buyerId,
        decimal amount)
    {
        decimal reusableHeldAmount =
            previousLeadingBid?.BuyerId == buyerId
                ? previousLeadingBid.Amount
                : 0;

        if (buyerWallet.AvailableBalance + reusableHeldAmount < amount)
        {
            throw new ApiException(
                StatusCodes.Status422UnprocessableEntity,
                "el comprador no posee saldo disponible suficiente.");
        }
    }

    public bool ShouldApplyAntiSniping(
        Auction auction,
        DateTime nowUtc)
    {
        return auction.EndAtUtc - nowUtc <= TimeSpan.FromSeconds(60);
    }
    public void ValidatePositiveAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el monto de la puja debe ser mayor a cero.");
        }
    }
}
