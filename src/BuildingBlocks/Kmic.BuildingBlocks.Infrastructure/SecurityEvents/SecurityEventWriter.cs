using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Application.Serialization;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 写安全事件：补全身份和时间，放进队列就返回
/// </summary>
/// <remarks>
/// 不等待、不抛异常：写不进去只记日志，不能因为记录安全事件让原来的请求失败
/// </remarks>
internal sealed class SecurityEventWriter(IExecutionContext execution, TimeProvider time, SecurityEventQueue queue, ILogger<SecurityEventWriter> logger) : ISecurityEventWriter
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "记录安全事件不能让原来的请求失败，出任何错都只记日志")]
    public void Write(SecurityEvent securityEvent)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);

        try
        {
            var record = Build(securityEvent);
            if (!queue.TryWrite(record))
            {
                SecurityEventLog.Dropped(logger, record.Type, record.Severity, record.Reason);
            }
        }
        catch (Exception exception)
        {
            SecurityEventLog.RecordFailed(logger, exception, securityEvent.Type);
        }
    }

    private static string? Cut(string? value, int length) => value is { } text && text.Length > length ? text[..length] : value;

    private SecurityEventRecord Build(SecurityEvent securityEvent)
    {
        var user = execution.User;
        return new SecurityEventRecord
        {
            Id = Guid.CreateVersion7(),
            OccurredAt = time.GetUtcNow(),
            Type = Cut(securityEvent.Type, 100)!,
            Severity = (short)securityEvent.Severity,
            Reason = Cut(securityEvent.Reason, 200),
            UserId = securityEvent.UserId ?? user.UserId?.Value,
            ActualUserId = user.ActualUserId?.Value,
            RoleId = user.RoleId?.Value,
            SessionId = user.SessionId?.Value,
            AttemptedAccount = Cut(securityEvent.AttemptedAccount, 100),
            ResourceType = Cut(securityEvent.ResourceType, 200),
            ResourceId = securityEvent.ResourceId,
            Details = securityEvent.Details is { Count: > 0 } details ? JsonSerializer.Serialize(details, KmicJsonOptions.Stored) : null,
            TraceId = Activity.Current?.TraceId.ToString() ?? new string('0', 32),
        };
    }
}
