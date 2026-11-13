using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Notifications;

namespace Kmic.Notifications.Application.Templates;

/// <summary>
/// 代码里声明的哈希：声明或默认内容一变，哈希就变，同步时据此判断要不要更新
/// </summary>
internal static class TemplateHash
{
    public static string Of(NotificationTemplateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var text = $"{definition.Key}\n{definition.DisplayName}\n{definition.Module}\n{(int)definition.Channels}\n{definition.SenderProfile}\n{definition.CarriesSecret}\n{definition.DefaultSubject}\n{definition.DefaultBody}\n{definition.DefaultTextBody}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}
