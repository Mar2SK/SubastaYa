using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Dtos.Wallets;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/wallets")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(WalletBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance(int userId)
    {
        WalletBalanceDto balance =
            await _walletService.GetBalanceAsync(userId);

        return Ok(balance);
    }

    [HttpPost("{userId:int}/transactions")]
    [ProducesResponseType(typeof(WalletBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletBalanceDto>> Deposit(
        int userId,
        [FromBody] DepositRequestDto request)
    {
        WalletBalanceDto balance =
            await _walletService.DepositAsync(userId, request);

        return Ok(balance);
    }
    [HttpGet("{userId:int}/transactions")]
    [ProducesResponseType(
    typeof(List<WalletTransactionItemDto>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<WalletTransactionItemDto>>> GetTransactions(
    int userId)
    {
        List<WalletTransactionItemDto> transactions =
            await _walletService.GetTransactionsAsync(userId);

        return Ok(transactions);
    }
}