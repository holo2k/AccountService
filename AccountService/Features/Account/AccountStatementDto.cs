using AccountService.Features.Transaction;

namespace AccountService.Features.Account;

public class AccountStatementDto
{
    public Guid AccountId { get; set; }
    public Guid OwnerId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}