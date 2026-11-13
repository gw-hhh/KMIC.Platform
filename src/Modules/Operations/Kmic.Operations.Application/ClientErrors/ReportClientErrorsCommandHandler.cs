using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 处理 <see cref="ReportClientErrorsCommand"/>
/// </summary>
public sealed class ReportClientErrorsCommandHandler(
    IExecutionContext execution,
    IClientErrorLimiter limiter,
    IClientErrorLog log,
    IOptions<ClientErrorOptions> options) : ICommandHandler<ReportClientErrorsCommand>
{
    /// <inheritdoc />
    public ValueTask<Result> Handle(ReportClientErrorsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var max = options.Value.MaxPerRequest;
        if (command.Errors.Count > max)
        {
            return ValueTask.FromResult<Result>(CommonErrors.ValidationFailedWith([new FieldError("errors", "MaxCount", $"一次最多上报 {max} 条")]));
        }

        var user = execution.User.UserId ?? UserId.System;
        if (limiter.TryAcquire(user) is { } retryAfter)
        {
            return ValueTask.FromResult<Result>(OperationsErrors.ClientErrorsThrottled(retryAfter));
        }

        foreach (var report in command.Errors)
        {
            log.Write(user, report);
        }

        return ValueTask.FromResult(Result.Success());
    }
}
