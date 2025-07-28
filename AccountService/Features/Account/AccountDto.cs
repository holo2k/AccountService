namespace AccountService.Features.Account;

public class AccountDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = string.Empty;

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public decimal Balance { get; set; }
    public decimal? PercentageRate { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime OpenDate { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime? CloseDate { get; set; }
}