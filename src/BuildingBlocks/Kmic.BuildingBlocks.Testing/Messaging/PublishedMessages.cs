using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Messaging;

/// <summary>
/// 读出发布方 Outbox 里的消息，断言命令发布了什么
/// </summary>
public static class PublishedMessages
{
    /// <summary>
    /// 某种消息的全部发布记录，按发布顺序
    /// </summary>
    public static async Task<IReadOnlyList<PublishedMessage<T>>> PublishedAsync<T>(this IServiceProvider services, CancellationToken ct = default)
        where T : class, IIntegrationMessage
    {
        ArgumentNullException.ThrowIfNull(services);

        var type = services.GetRequiredService<IntegrationMessageCatalog>().Describe(typeof(T));
        var outbox = services.GetRequiredService<ModuleMessageTables>().For(PublisherOf(services, type)).Outbox;
        await using var command = services.GetRequiredService<DataSources>().Reads(CallOrigin.Inbox).CreateCommand(
            $"""
            SELECT "Id", "SchemaVersion", "Payload"::text, "Headers"::text, "OccurredAt", "SourceId", "SourceVersion"
            FROM {outbox} WHERE "MessageType" = @type ORDER BY "Seq"
            """);
        command.Parameters.AddWithValue("type", type.Name);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var messages = new List<PublishedMessage<T>>();
        while (await reader.ReadAsync(ct))
        {
            var headers = JsonSerializer.Deserialize<MessageHeaders>(reader.GetString(3), KmicJsonOptions.Stored)
                ?? throw new InvalidOperationException("信封是 null。");
            var envelope = new MessageEnvelope(
                reader.GetGuid(0), type.Name, reader.GetInt32(1), headers.SourceModule, await reader.GetFieldValueAsync<DateTimeOffset>(4, ct),
                await reader.GetFieldValueAsync<Guid?>(5, ct), await reader.GetFieldValueAsync<long?>(6, ct), headers.Purged, headers.InitiatorUserId);
            var message = JsonSerializer.Deserialize<T>(reader.GetString(2), KmicJsonOptions.Stored) ?? throw new InvalidOperationException("消息内容是 null。");
            messages.Add(new PublishedMessage<T>(message, envelope, headers.CausationId));
        }

        return messages;
    }

    // 目录里的模块名是定义消息的模块；框架消息（Framework.*）由各模块自己发布，这里不支持
    private static string PublisherOf(IServiceProvider services, IntegrationMessageType type) =>
        services.GetRequiredService<ModuleMessageTables>().Contains(type.Module)
            ? type.Module
            : throw new InvalidOperationException($"{type.Name} 由 {type.Module} 模块定义，这个宿主没有注册它的写入上下文。");
}
