using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 注册 KmicFunctions 的翻译器
/// </summary>
internal sealed class KmicFunctionsExtension : IDbContextOptionsExtension
{
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) => services.AddScoped<IMethodCallTranslatorPlugin, Plugin>();

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class Plugin(ISqlExpressionFactory sql) : IMethodCallTranslatorPlugin
    {
        public IEnumerable<IMethodCallTranslator> Translators { get; } = [new KmicFunctionsTranslator((NpgsqlSqlExpressionFactory)sql)];
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "KmicFunctions ";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) => debugInfo["Kmic:Functions"] = "1";
    }
}
