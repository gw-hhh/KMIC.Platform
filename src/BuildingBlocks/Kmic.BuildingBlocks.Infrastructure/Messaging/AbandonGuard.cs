using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 按消息名称解析策略并校验是否允许放弃（未注册或载荷损坏时拒绝）
internal sealed class AbandonGuard(IntegrationMessageCatalog messages, IExecutionScopeFactory scopes) : IAbandonGuard
{
    private static readonly ConcurrentDictionary<Type, (Type Policy, MethodInfo Check)> _policies = new();

    public async Task<Result> CheckAsync(DeadMessageDetail message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (messages.Find(message.Message.MessageType, message.Message.SchemaVersion) is not { } type)
        {
            return CommonErrors.MessageUnknown;
        }

        var (policyType, check) = _policies.GetOrAdd(type.ClrType, clr =>
        {
            var policy = typeof(IAbandonPolicy<>).MakeGenericType(clr);
            return (policy, policy.GetMethod(nameof(IAbandonPolicy<>.CheckAsync))!);
        });

        await using var scope = scopes.CreateScope(CallOrigin.Inbox, CurrentUser.System());
        if (scope.ServiceProvider.GetService(policyType) is not { } policy)
        {
            return Result.Success();
        }

        object? payload;
        try
        {
            payload = JsonSerializer.Deserialize(message.Payload, type.ClrType, KmicJsonOptions.Stored);
        }
        catch (JsonException)
        {
            return CommonErrors.MessagePayloadUnreadable;
        }

        return payload is null ? CommonErrors.MessagePayloadUnreadable : await (ValueTask<Result>)check.Invoke(policy, [payload, ct])!;
    }
}
