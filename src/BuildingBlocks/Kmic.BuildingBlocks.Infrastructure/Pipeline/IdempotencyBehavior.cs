using System.Data.Common;
using System.Security.Cryptography;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 幂等处理
/// </summary>
/// <remarks>
/// 只处理标了 [Idempotent] 的 HTTP 命令：同一个幂等键只执行一次，重复请求返回第一次的结果。
/// 完成标记和业务数据在同一个事务里提交，否则两次提交之间崩溃，就会出现业务已生效、记录却还是处理中
/// </remarks>
public sealed class IdempotencyBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    // 从容器取框架服务：宿主要直接引用行为类型，所以行为只能是 public，构造函数里就放不了 internal 的服务
    IServiceProvider services) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseCommand
    where TResponse : Result, IResultFactory<TResponse>
{
    private const int MaxKeyLength = 100;

    private static readonly Error _inProgress = CommonErrors.RequestInProgress.WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        // 非 HTTP 来源不走这里：Inbox 按事件 ID 去重，后台作业按作业 ID 领取
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
            ?? throw new InvalidOperationException($"{useCase.Name} 标了 [Idempotent]，但请求没有用户身份。匿名用例不能声明幂等。");
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
            // 提交结果未知时按记录判断：还能删掉自己的记录，说明没有提交，交给外层提示用原键重试
            if (await ResolveAsync(store, id, token, failure: null) is { } resolved)
            {
                return resolved;
            }

            throw;
        }
        catch
        {
            // 包括客户端断开引起的取消：业务事务已回滚，释放记录后同一个键马上可以重试
            await TryReleaseAsync(store, id, token);
            throw;
        }

        return response.IsSuccess ? response : await ResolveAsync(store, id, token, response) ?? response;
    }

    private static string? Serialize(object? value) => value is null ? null : JsonSerializer.Serialize(value, value.GetType(), IdempotencyJson.Options);

    // 失败时删掉自己的记录；删不掉就重新读取再判断，不猜测结果
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

    // 尽量删掉记录。删不掉就等占用到期，由同一个键的下一个请求接管
    private static async Task TryReleaseAsync(IdempotencyStore store, Guid id, Guid token)
    {
        try
        {
            await store.TryReleaseAsync(id, token, CancellationToken.None);
        }
        catch (DbException)
        {
            // 原来的异常更重要，继续往外抛它
        }
    }

    private TResponse Replay(IdempotencySnapshot record)
    {
        services.GetRequiredService<RequestMetadata>().Replayed = true;
        var value = record.ResponseBody is null || ResponseValues<TResponse>.ValueType is not { } type
            ? null
            : JsonSerializer.Deserialize(record.ResponseBody, type, IdempotencyJson.Options);
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
            RequestHash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(message, IdempotencyJson.Options)),
            LeaseToken = Guid.CreateVersion7(),
            LeaseExpiresAt = now + options.LeaseDuration,
            CreatedAt = now,
            ExpiresAt = now + options.Retention,
        };
    }

    private async Task<Claim> ClaimAsync(IdempotencyStore store, NewIdempotencyRecord record, CancellationToken ct)
    {
        // 只有读到过期的旧记录并删掉之后，才会进入第二轮
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
