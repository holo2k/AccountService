using System.Text.Json.Serialization;

namespace AccountService.Features.Account
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public AccountType Type { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal? PercentageRate { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }

        public virtual ICollection<Transaction.Transaction> Transactions { get; set; }
    }
}
