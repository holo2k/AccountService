namespace AccountService.Features.Health;

public record HealthDto(bool RabbitAlive, int OutboxPending, string? Warning);