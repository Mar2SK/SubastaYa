using Microsoft.AspNetCore.Http;
using SubastaYa.Api.Dtos.Wallets;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Models;
using SubastaYa.Api.Repositories;

namespace SubastaYa.Api.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<WalletBalanceDto> GetBalanceAsync(int userId)
    {
        Wallet? wallet = await _walletRepository.GetByUserIdAsync(userId);

        if (wallet is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la billetera del usuario no existe.");
        }

        return MapBalance(wallet);
    }

    public async Task<WalletBalanceDto> DepositAsync(
        int userId,
        DepositRequestDto request)
    {
        if (request.Amount <= 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el monto a acreditar debe ser mayor a cero.");
        }

        Wallet? wallet = await _walletRepository.DepositAsync(
            userId,
            request.Amount);

        if (wallet is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la billetera del usuario no existe.");
        }

        return MapBalance(wallet);
    }

    private static WalletBalanceDto MapBalance(Wallet wallet)
    {
        return new WalletBalanceDto
        {
            WalletId = wallet.Id,
            UserId = wallet.UserId,
            TotalBalance = wallet.TotalBalance,
            HeldBalance = wallet.HeldBalance,
            AvailableBalance = wallet.AvailableBalance
        };
    }
    public async Task<List<WalletTransactionItemDto>> GetTransactionsAsync(
    int userId)
    {
        Wallet? wallet = await _walletRepository.GetByUserIdAsync(userId);

        if (wallet is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la billetera del usuario no existe.");
        }

        return await _walletRepository.GetTransactionsByUserIdAsync(userId);
    }
}