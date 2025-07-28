namespace AccountService.Features.Account.AddAccount;

public class AddAccountRequest
{
    public Guid OwnerId { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal? PercentageRate { get; set; }
}