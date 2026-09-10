using SubastaYa.Api.Dtos.Wallets;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(int userId);

    Task<Wallet?> DepositAsync(int userId, decimal amount);

    Task<List<WalletTransactionItemDto>> GetTransactionsByUserIdAsync(
        int userId);
}