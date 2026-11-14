using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Notifications;

namespace Kmic.Notifications.Application.Templates;

/// <summary>
/// 模板同步用的哈希，包含声明属性和默认内容
/// </summary>
/// <remarks>
/// 不包含模型结构和示例数据
/// </remarks>
internal static class TemplateHash
{
    public static string Of(NotificationTemplateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var text = $"{definition.Key}\n{definition.DisplayName}\n{definition.Module}\n{(int)definition.Channels}\n{definition.SenderProfile}\n{definition.CarriesSecret}\n{definition.DefaultSubject}\n{definition.DefaultBody}\n{definition.DefaultTextBody}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}
