namespace AccountService.Features.Outbox;

public class InboxConsumed
{
    public Guid MessageId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Handler { get; set; } = null!;
}