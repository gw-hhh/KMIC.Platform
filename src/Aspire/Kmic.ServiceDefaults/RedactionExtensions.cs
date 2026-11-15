using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.ServiceDefaults;

/// <summary>
/// 注册数据分类脱敏器
/// </summary>
public static class RedactionExtensions
{
    /// <summary>
    /// 注册按数据分类脱敏的服务
    /// </summary>
    /// <remarks>
    /// Secret 输出 ***，Personal 部分遮盖，Internal 原样；调用方通过 IRedactorProvider 获取脱敏器
    /// </remarks>
    public static IServiceCollection AddKmicRedaction(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddRedaction(redaction => redaction
            .SetRedactor<SecretRedactor>(KmicData.Secret)
            .SetRedactor<PersonalRedactor>(KmicData.Personal)
            .SetRedactor<InternalRedactor>(KmicData.Internal));
    }
}
