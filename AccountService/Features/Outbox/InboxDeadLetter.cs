namespace AccountService.Features.Outbox;

public class InboxDeadLetter
{
    public Guid MessageId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Handler { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public string Error { get; set; } = null!;
}