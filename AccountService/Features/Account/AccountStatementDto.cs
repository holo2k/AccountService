using AccountService.Features.Transaction;

namespace AccountService.Features.Account;

public class AccountStatementDto
{
    public Guid AccountId { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public Guid OwnerId { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public string Currency { get; set; } = string.Empty;

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public AccountType Type { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public decimal Balance { get; set; }
    public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}