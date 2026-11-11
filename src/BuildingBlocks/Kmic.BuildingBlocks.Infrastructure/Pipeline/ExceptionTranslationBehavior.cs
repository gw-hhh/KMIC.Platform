using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 异常转换
/// </summary>
/// <remarks>
/// 把并发冲突、唯一约束冲突这类数据库异常转成错误结果，其他异常照常抛出。放在事务外层，提交时抛出的异常也能转换
/// </remarks>
public sealed class ExceptionTranslationBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    ILogger<ExceptionTranslationBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        try
        {
            return await next(message, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            return TResponse.Fail(await VersionConflictAsync(exception));
        }
        catch (Exception exception) when (FindPostgres(exception) is { } postgres && Translate(postgres) is { } error)
        {
            return TResponse.Fail(error);
        }
        catch (CommitOutcomeUnknownException exception)
        {
            // 幂等命令能走到这里，说明内层已确认没有提交，可以用原键重试。非幂等命令提示刷新后确认
            PipelineLog.CommitOutcomeUnknown(logger, exception, typeof(TMessage).Name);
            return TResponse.Fail(CommonErrors.ResultUnknown.WithMetadata(ErrorMetadataKeys.Retryable, catalog.Get(typeof(TMessage)).Idempotent));
        }
    }

    private static PostgresException? FindPostgres(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres)
            {
                return postgres;
            }
        }

        return null;
    }

    // 错误里带上聚合根的当前版本号，数据已被删除时不带
    private static async Task<Error> VersionConflictAsync(DbUpdateConcurrencyException exception)
    {
        if (exception.Entries.FirstOrDefault(e => e.Entity is IAggregateRoot) is not { } root
            || CurrentVersionQuery(root) is not { } query)
        {
            return CommonErrors.VersionConflict;
        }

        var versions = await root.Context.Database.SqlQueryRaw<long>(query.Sql, new NpgsqlParameter("id", query.Id)).ToListAsync(CancellationToken.None);
        return versions.Count == 1
            ? CommonErrors.VersionConflict.WithMetadata(ErrorMetadataKeys.CurrentVersion, versions[0])
            : CommonErrors.VersionConflict;
    }

    private static (string Sql, Guid Id)? CurrentVersionQuery(EntityEntry root)
    {
        if (root.Property(nameof(Entity<>.Id)).CurrentValue is not IStronglyTypedId id)
        {
            return null;
        }

        var entityType = root.Metadata;
        var sql = $"""SELECT "{nameof(IAggregateRoot.ConcurrencyVersion)}" AS "Value" FROM "{entityType.GetSchema()}"."{entityType.GetTableName()}" WHERE "Id" = @id""";
        return (sql, id.Value);
    }

    private Error? Translate(PostgresException exception) => exception.SqlState switch
    {
        PostgresErrorCodes.UniqueViolation => UniqueViolation(exception),
        PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected =>
            CommonErrors.TransientConflict.WithMetadata(ErrorMetadataKeys.Retryable, true),
        _ => null,
    };

    private Error UniqueViolation(PostgresException exception)
    {
        if (UniqueConstraintErrors.Find(exception.SchemaName, exception.ConstraintName) is { } mapped)
        {
            return mapped;
        }

        PipelineLog.UnmappedUniqueViolation(logger, exception.SchemaName, exception.ConstraintName, typeof(TMessage).Name);
        return CommonErrors.DuplicateValue;
    }
}
