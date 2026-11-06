using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Spike06.Tests;

// 业务时区随选项扩展进入 EF 的内部服务；时区不同的上下文使用不同的内部服务提供者
internal sealed class KmicFunctionsExtension(string businessTimeZone) : IDbContextOptionsExtension
{
    public string BusinessTimeZone { get; } = businessTimeZone;

    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) =>
        services.AddScoped<IMethodCallTranslatorPlugin, Plugin>();

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class Plugin(ISqlExpressionFactory sql, IRelationalTypeMappingSource typeMappings, IDbContextOptions options) : IMethodCallTranslatorPlugin
    {
        public IEnumerable<IMethodCallTranslator> Translators { get; } =
            [new BusinessDateTranslator(sql, typeMappings, options.FindExtension<KmicFunctionsExtension>()!.BusinessTimeZone)];
    }

    private sealed class ExtensionInfo(KmicFunctionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => $"BusinessTimeZone={extension.BusinessTimeZone} ";

        public override int GetServiceProviderHashCode() => StringComparer.Ordinal.GetHashCode(extension.BusinessTimeZone);

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other is ExtensionInfo info && string.Equals(info.Extension is KmicFunctionsExtension e ? e.BusinessTimeZone : null, extension.BusinessTimeZone, StringComparison.Ordinal);

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) => debugInfo["Kmic:BusinessTimeZone"] = extension.BusinessTimeZone;
    }
}
