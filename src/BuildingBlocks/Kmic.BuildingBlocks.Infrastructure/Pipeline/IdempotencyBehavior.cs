using System.Data.Common;
using System.Security.Cryptography;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>确保同一幂等键仅执行一次，重复请求直接返回首次执行结果</summary>
public sealed class IdempotencyBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    // 从服务容器解析框架内部服务：由于宿主需直接引用行为类型（必须为 public），构造函数无法直接注入 internal 服务
    IServiceProvider services) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : Result, IResultFactory<TResponse>
{
    private const int MaxKeyLength = 100;

    private static readonly Error _inProgress = CommonErrors.RequestInProgress.WithMetadata(ErrorMetadataKeys.Retryable, true);

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        // 非 HTTP 来源不经过此行为：Inbox 按事件 ID 去重，后台作业按作业 ID 领取
        var useCase = catalog.Get(typeof(TMessage));
        var execution = services.GetRequiredService<ExecutionScope>();
        if (!useCase.Idempotent || execution.Origin != CallOrigin.Http)
        {
            return await next(message, cancellationToken);
        }

        var key = services.GetRequiredService<RequestMetadata>().IdempotencyKey;
        if (string.IsNullOrEmpty(key) || key.Length > MaxKeyLength)
        {
            return TResponse.Fail(CommonErrors.IdempotencyKeyRequired.WithDetail($"请求头 Idempotency-Key 必填，1～{MaxKeyLength} 个字符。"));
        }

        var userId = execution.User.UserId
            ?? throw new InvalidOperationException($"{useCase.Name} 标注了 [Idempotent]，但请求缺少用户身份；匿名用例不可声明幂等。");
        var context = services.GetRequiredService<ModuleWriteContexts>().Get(useCase.Module);
        var store = IdempotencyStore.For(context, services.GetRequiredService<DataSources>().Transactions(execution.EntryOrigin));
        var claim = await ClaimAsync(store, NewRecord(useCase, userId.Value, execution.User.RoleId?.Value ?? Guid.Empty, key, message), cancellationToken);
        if (claim.Response is { } decided)
        {
            return decided;
        }

        var (id, token) = (claim.Id, claim.Token);
        var time = services.GetRequiredService<TimeProvider>();
        services.GetRequiredService<CommandTransaction>().BeforeCommit(async (db, value, ct) =>
            await store.TryCompleteAsync(db, id, token, Serialize(value), time.GetUtcNow(), ct) ? null : _inProgress);

        TResponse response;
        try
        {
            response = await next(message, cancellationToken);
        }
        catch (CommitOutcomeUnknownException)
        {
            // 提交结果未知时根据幂等记录判定：若仍可删除本请求写入的记录，说明事务未提交，交由外层提示使用原键重试
            if (await ResolveAsync(store, id, token, failure: null) is { } resolved)
            {
                return resolved;
            }

            throw;
        }
        catch
        {
            // 包括客户端断开导致的取消：此时业务事务已回滚，释放幂等记录后即可使用同一幂等键立即重试
            await TryReleaseAsync(store, id, token);
            throw;
        }

        return response.IsSuccess ? response : await ResolveAsync(store, id, token, response) ?? response;
    }

    private static string? Serialize(object? value) => value is null ? null : JsonSerializer.Serialize(value, value.GetType(), KmicJsonOptions.Stored);

    // 执行失败时删除当前请求的幂等记录；若删除失败则重新读取记录状态以确定结果，不做猜测
    private async Task<TResponse?> ResolveAsync(IdempotencyStore store, Guid id, Guid token, TResponse? failure)
    {
        try
        {
            if (await store.TryReleaseAsync(id, token, CancellationToken.None))
            {
                return failure;
            }

            var record = await store.FindByIdAsync(id, CancellationToken.None);
            return record switch
            {
                null => failure,
                { Status: IdempotencyStore.Completed } => Replay(record),
                _ => TResponse.Fail(_inProgress),
            };
        }
        catch (DbException)
        {
            return TResponse.Fail(CommonErrors.ResultUnknown.WithMetadata(ErrorMetadataKeys.Retryable, true));
        }
    }

    // 尝试清理幂等记录；若清理失败，则等待占用过期后由同一幂等键的后续请求接管
    private static async Task TryReleaseAsync(IdempotencyStore store, Guid id, Guid token)
    {
        try
        {
            await store.TryReleaseAsync(id, token, CancellationToken.None);
        }
        catch (DbException)
        {
            // 保留并重新抛出原始异常
        }
    }

    private TResponse Replay(IdempotencySnapshot record)
    {
        services.GetRequiredService<RequestMetadata>().Replayed = true;
        var value = record.ResponseBody is null || ResponseValues<TResponse>.ValueType is not { } type
            ? null
            : JsonSerializer.Deserialize(record.ResponseBody, type, KmicJsonOptions.Stored);
        return ResponseValues<TResponse>.Create(value);
    }

    private NewIdempotencyRecord NewRecord(UseCase useCase, Guid userId, Guid roleId, string key, TMessage message)
    {
        var options = services.GetRequiredService<IOptions<IdempotencyOptions>>().Value;
        var now = services.GetRequiredService<TimeProvider>().GetUtcNow();
        return new NewIdempotencyRecord
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            RoleId = roleId,
            Operation = useCase.Name,
            Key = key,
            RequestHash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(message, KmicJsonOptions.Default)),
            LeaseToken = Guid.CreateVersion7(),
            LeaseExpiresAt = now + options.LeaseDuration,
            CreatedAt = now,
            ExpiresAt = now + options.Retention,
        };
    }

    private async Task<Claim> ClaimAsync(IdempotencyStore store, NewIdempotencyRecord record, CancellationToken ct)
    {
        // 仅当读取并成功清理已过期的旧记录后，才进入第二轮重试
        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (await store.TryClaimAsync(record, ct))
            {
                return new Claim(record.Id, record.LeaseToken, null);
            }

            var existing = await store.FindAsync(record, ct);
            if (existing is null)
            {
                continue;
            }

            if (existing.ExpiresAt <= record.CreatedAt)
            {
                await store.DeleteExpiredAsync(existing.Id, record.CreatedAt, ct);
                continue;
            }

            if (!CryptographicOperations.FixedTimeEquals(existing.RequestHash, record.RequestHash))
            {
                return Decided(TResponse.Fail(CommonErrors.IdempotencyKeyReused));
            }

            if (string.Equals(existing.Status, IdempotencyStore.Completed, StringComparison.Ordinal))
            {
                return Decided(Replay(existing));
            }

            if (existing.LeaseExpiresAt <= record.CreatedAt
                && await store.TryTakeOverAsync(existing, record.LeaseToken, record.LeaseExpiresAt, record.CreatedAt, ct))
            {
                return new Claim(existing.Id, record.LeaseToken, null);
            }

            return Decided(TResponse.Fail(_inProgress));
        }

        return Decided(TResponse.Fail(_inProgress));
    }

    private static Claim Decided(TResponse response) => new(Guid.Empty, Guid.Empty, response);

    private sealed record Claim(Guid Id, Guid Token, TResponse? Response);
}
