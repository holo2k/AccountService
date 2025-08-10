using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Features.Account;

public class Account
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public decimal? PercentageRate { get; set; }
    public DateTime OpenDate { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime? CloseDate { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public virtual ICollection<Transaction.Transaction> Transactions { get; set; } =
        new List<Transaction.Transaction>();

    [Timestamp]
    [Column("xmin", TypeName = "xid")]
    public uint Version { get; set; }
}