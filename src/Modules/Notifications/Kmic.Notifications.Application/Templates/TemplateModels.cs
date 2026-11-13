using System.Text.Json;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Application.Templates;

/// <summary>
/// 把 JSON 读成模板声明的数据模型
/// </summary>
internal static class TemplateModels
{
    public static Result<object> Read(NotificationTemplateDefinition definition, JsonElement json)
    {
        ArgumentNullException.ThrowIfNull(definition);

        try
        {
            return JsonSerializer.Deserialize(json.GetRawText(), definition.ModelType, NotificationModelJson.Options) is { } model
                ? model
                : NotificationsErrors.ModelInvalid(definition.Key);
        }
        catch (JsonException)
        {
            return NotificationsErrors.ModelInvalid(definition.Key);
        }
    }
}
