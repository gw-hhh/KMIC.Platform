using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 从登记的模块里选出本次要运行的，并校验它们
/// </summary>
/// <remarks>
/// 问题汇总后一次报出，任何一条都拒绝启动：模块名配对、程序集归属、模块名不重复、依赖已注册且已启用、
/// 配置里关掉的模块被别的已启用模块依赖
/// </remarks>
internal static class ModuleSelection
{
    // checkAssemblyNames 只给测试用：测试里的模块没法放进名为 Kmic.{模块}.Application 的程序集
    // 参考模块只能在开发与测试环境注册，生产环境即使漏写 onlyIn 也不能带着它启动
    private const string ReferenceModule = "Sample";

    public static IReadOnlyList<HostModule> Select(IReadOnlyList<HostModule> registered, IHostEnvironment environment, IConfiguration configuration, bool checkAssemblyNames = true)
    {
        var problems = new List<string>();
        CheckPairs(registered, checkAssemblyNames, problems);

        var enabled = registered
            .Where(m => m.OnlyIn != KmicEnvironments.NonProduction || !environment.IsProduction())
            .Where(m => configuration.GetValue<bool?>($"Modules:{m.Module.Name}:Enabled") ?? true)
            .ToList();

        if (environment.IsProduction() && enabled.Exists(m => string.Equals(m.Module.Name, ReferenceModule, StringComparison.Ordinal)))
        {
            problems.Add($"生产环境不能注册参考模块 {ReferenceModule}：注册时写 onlyIn: KmicEnvironments.NonProduction。");
        }

        CheckDependencies(registered, enabled, problems);

        return problems.Count > 0 ? throw new DeclarationException("模块注册", problems) : enabled;
    }

    private static void CheckPairs(IReadOnlyList<HostModule> registered, bool checkAssemblyNames, List<string> problems)
    {
        problems.AddRange(registered.GroupBy(m => m.Module.Name, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => $"模块 {g.Key} 注册了 {g.Count()} 次。"));

        foreach (var module in registered)
        {
            var name = module.Module.Name;
            if (!string.Equals(name, module.Endpoints.Module, StringComparison.Ordinal))
            {
                problems.Add($"模块 {name}：服务端注册类 {module.Module.GetType().Name} 与端点注册类 {module.Endpoints.GetType().Name} 的模块名不一致（{name} / {module.Endpoints.Module}）。");
            }

            if (!checkAssemblyNames)
            {
                continue;
            }

            var application = module.Module.ApplicationAssembly.GetName().Name;
            if (!string.Equals(application, $"Kmic.{name}.Application", StringComparison.Ordinal))
            {
                problems.Add($"模块 {name}：Application 程序集应当叫 Kmic.{name}.Application，现在是 {application}。");
            }

            var presentation = module.Endpoints.GetType().Assembly.GetName().Name;
            if (!string.Equals(presentation, $"Kmic.{name}.Presentation", StringComparison.Ordinal))
            {
                problems.Add($"模块 {name}：端点注册类应当在 Kmic.{name}.Presentation 程序集里，现在在 {presentation}。");
            }
        }
    }

    private static void CheckDependencies(IReadOnlyList<HostModule> registered, List<HostModule> enabled, List<string> problems)
    {
        var enabledNames = enabled.Select(m => m.Module.Name).ToHashSet(StringComparer.Ordinal);
        var registeredNames = registered.Select(m => m.Module.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var module in enabled)
        {
            var dependencies = new DependencyList();
            module.Module.DeclareDependencies(dependencies);
            foreach (var dependency in dependencies.Names.Where(d => !enabledNames.Contains(d)))
            {
                problems.Add(registeredNames.Contains(dependency)
                    ? $"模块 {module.Module.Name} 依赖的模块 {dependency} 没有启用（配置里关闭了，或者只在非生产环境注册）。"
                    : $"模块 {module.Module.Name} 依赖的模块 {dependency} 没有注册。");
            }
        }
    }

    private sealed class DependencyList : IModuleDependencies
    {
        private readonly List<string> _names = [];

        public IReadOnlyList<string> Names => _names;

        public void DependsOn(string module) => _names.Add(module);
    }
}
