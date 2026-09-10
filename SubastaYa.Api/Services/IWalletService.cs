using SubastaYa.Api.Dtos.Wallets;

namespace SubastaYa.Api.Services;

public interface IWalletService
{
    Task<WalletBalanceDto> GetBalanceAsync(int userId);

    Task<WalletBalanceDto> DepositAsync(
        int userId,
        DepositRequestDto request);

    Task<List<WalletTransactionItemDto>> GetTransactionsAsync(
    int userId);
}