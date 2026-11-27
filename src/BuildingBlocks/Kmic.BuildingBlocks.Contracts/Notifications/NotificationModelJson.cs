using System.Text.Json;

namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>通知数据模型的 JSON 序列化配置（发送方与 Notifications 模块必须共用同一份选项）。</summary>
public static class NotificationModelJson
{
    /// <summary>共用序列化选项。</summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerOptions.Strict)
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = false,
    };

    /// <summary>将通知模型序列化为 JSON。</summary>
    public static JsonElement Serialize<TModel>(TModel model)
        where TModel : INotificationModel<TModel> =>
        JsonSerializer.SerializeToElement(model, Options);
}
