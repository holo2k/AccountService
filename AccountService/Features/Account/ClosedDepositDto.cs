namespace AccountService.Features.Account;

public class ClosedDepositDto
{
    public Guid AccountId { get; set; }
    public DateTime CloseDate { get; set; }
    public decimal Balance { get; set; }
}