namespace SubastaYa.Api.Services;

public interface IAuctionActivationService
{
    Task ProcessScheduledAuctionsAsync();
}