using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.ServiceDefaults;

public static class RedactionExtensions
{
    /// <summary>注册基于数据分类的日志与数据脱敏器</summary>
    public static IServiceCollection AddKmicRedaction(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddRedaction(redaction => redaction
            .SetRedactor<SecretRedactor>(KmicData.Secret)
            .SetRedactor<PersonalRedactor>(KmicData.Personal)
            .SetRedactor<InternalRedactor>(KmicData.Internal));
    }
}
