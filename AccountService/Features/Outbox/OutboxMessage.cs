namespace AccountService.Features.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public int? PublishedLatencyMs { get; set; }
}