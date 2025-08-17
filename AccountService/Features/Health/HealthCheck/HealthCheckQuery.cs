using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Health.HealthCheck;

public record HealthCheckQuery : IRequest<MbResult<HealthDto>>;