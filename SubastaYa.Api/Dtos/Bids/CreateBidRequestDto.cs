namespace SubastaYa.Api.Dtos.Bids;

public class CreateBidRequestDto
{
    public int BuyerId { get; set; }

    public decimal Amount { get; set; }
}