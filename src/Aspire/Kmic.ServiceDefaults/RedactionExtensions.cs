using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.ServiceDefaults;

/// <summary>
/// 数据分类的脱敏器注册
/// </summary>
public static class RedactionExtensions
{
    /// <summary>
    /// 注册按 <see cref="KmicData" /> 分类取脱敏器的提供者
    /// </summary>
    /// <remarks>
    /// Secret 输出 ***，Personal 部分遮盖，Internal 原样。需要把某个值按分类脱敏的服务注入 IRedactorProvider
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
