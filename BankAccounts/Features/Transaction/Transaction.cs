namespace AccountService.Features.Transaction
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

        public Guid counterpartyAccountId { get; set; }
        public Decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
