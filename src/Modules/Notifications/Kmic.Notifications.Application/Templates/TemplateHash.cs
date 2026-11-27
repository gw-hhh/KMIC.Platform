using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Notifications;

namespace Kmic.Notifications.Application.Templates;

// 仅纳入声明属性与默认内容，不含模型结构与示例数据
internal static class TemplateHash
{
    public static string Of(NotificationTemplateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var text = $"{definition.Key}\n{definition.DisplayName}\n{definition.Module}\n{(int)definition.Channels}\n{definition.SenderProfile}\n{definition.CarriesSecret}\n{definition.DefaultSubject}\n{definition.DefaultBody}\n{definition.DefaultTextBody}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}
