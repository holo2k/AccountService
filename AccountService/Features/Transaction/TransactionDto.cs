namespace AccountService.Features.Transaction;

public class TransactionDto
{
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public Guid? CounterPartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime Date { get; set; }
}