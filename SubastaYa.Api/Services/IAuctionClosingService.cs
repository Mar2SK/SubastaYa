namespace SubastaYa.Api.Services;

public interface IAuctionClosingService
{
    Task ProcessExpiredAuctionsAsync();
}