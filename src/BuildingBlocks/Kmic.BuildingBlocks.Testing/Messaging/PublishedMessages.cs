using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Messaging;

/// <summary>读取发布方发件箱记录，用于断言命令已发布的集成消息。</summary>
public static class PublishedMessages
{
    /// <summary>按发布顺序获取指定类型消息的全部发件记录。</summary>
    public static async Task<IReadOnlyList<PublishedMessage<T>>> PublishedAsync<T>(this IServiceProvider services, CancellationToken ct = default)
        where T : class, IIntegrationMessage
    {
        ArgumentNullException.ThrowIfNull(services);

        var type = services.GetRequiredService<IntegrationMessageCatalog>().Describe(typeof(T));
        var outbox = services.GetRequiredService<ModuleFrameworkTables>().For(PublisherOf(services, type)).Outbox;
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
                ?? throw new InvalidOperationException("消息信封反序列化结果为 null。");
            var envelope = new MessageEnvelope(
                reader.GetGuid(0), type.Name, reader.GetInt32(1), headers.SourceModule, await reader.GetFieldValueAsync<DateTimeOffset>(4, ct),
                await reader.GetFieldValueAsync<Guid?>(5, ct), await reader.GetFieldValueAsync<long?>(6, ct), headers.Purged, headers.InitiatorUserId);
            var message = JsonSerializer.Deserialize<T>(reader.GetString(2), KmicJsonOptions.Stored) ?? throw new InvalidOperationException("消息内容反序列化结果为 null。");
            messages.Add(new PublishedMessage<T>(message, envelope, headers.CausationId));
        }

        return messages;
    }

    // 目录中的模块名为消息定义方；框架消息（Framework.*）由各模块自行发布，此处暂不支持
    private static string PublisherOf(IServiceProvider services, IntegrationMessageType type) =>
        services.GetRequiredService<ModuleFrameworkTables>().Contains(type.Module)
            ? type.Module
            : throw new InvalidOperationException($"{type.Name} 由 {type.Module} 模块定义，当前宿主未注册其写入上下文。");
}
