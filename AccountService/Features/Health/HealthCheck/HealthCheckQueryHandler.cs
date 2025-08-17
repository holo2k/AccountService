using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Repository;
using AccountService.PipelineBehaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Features.Health.HealthCheck;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class HealthCheckQueryHandler : IRequestHandler<HealthCheckQuery, MbResult<HealthDto>>
{
    private readonly AppDbContext _db;
    private readonly IRabbitMqPublisher _rabbit;

    public HealthCheckQueryHandler(AppDbContext db, IRabbitMqPublisher rabbit)
    {
        _db = db;
        _rabbit = rabbit;
    }

    public async Task<MbResult<HealthDto>> Handle(HealthCheckQuery request, CancellationToken cancellationToken)
    {
        var rabbitAlive = await _rabbit.CheckConnectionAsync(cancellationToken);
        if (!rabbitAlive)
            return MbResult<HealthDto>.Fail(new MbError
            {
                Code = "NotReady",
                Message = "RabbitMq is unavailable"
            });

        if (!await _db.Database.CanConnectAsync(cancellationToken))
            return MbResult<HealthDto>.Fail(new MbError
            {
                Code = "NotReady",
                Message = "Database is unavailable"
            });

        var outboxPending = await _db.OutboxMessages.CountAsync(x => x.ProcessedAt == null, cancellationToken);
        var warning = outboxPending > 100 ? "WARN: >100 unprocessed outbox messages" : null;

        return MbResult<HealthDto>.Success(new HealthDto(rabbitAlive, outboxPending, warning));
    }
}