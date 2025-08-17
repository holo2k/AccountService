namespace AccountService.Features.Account;

public class AccrueInterestModel
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
}