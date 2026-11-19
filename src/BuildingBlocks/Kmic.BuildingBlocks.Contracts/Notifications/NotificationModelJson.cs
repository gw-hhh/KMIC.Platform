using System.Text.Json;

namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 通知数据模型在消息里的 JSON 写法
/// </summary>
/// <remarks>
/// 发送方和 Notifications 模块必须用同一份选项
/// </remarks>
public static class NotificationModelJson
{
    /// <summary>
    /// 序列化与反序列化共用的选项
    /// </summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerOptions.Strict)
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = false,
    };

    /// <summary>
    /// 把数据模型写成消息里的 JSON
    /// </summary>
    public static JsonElement Serialize<TModel>(TModel model)
        where TModel : INotificationModel<TModel> =>
        JsonSerializer.SerializeToElement(model, Options);
}
