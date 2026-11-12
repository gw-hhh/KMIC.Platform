using System.Reflection;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Kmic.BuildingBlocks.Hosting.Tests;

/// <summary>
/// 模块注册的校验：配对、重复、环境、开关、依赖
/// </summary>
public sealed class ModuleSelectionTests
{
    [Fact]
    public void ModuleAndEndpointsWithDifferentNames_AreRejected()
    {
        var registered = new[] { Pair("Alpha", endpoints: "Beta") };

        Problems(registered).ShouldContain(p => p.Contains("Alpha", StringComparison.Ordinal) && p.Contains("模块名不一致", StringComparison.Ordinal));
    }

    [Fact]
    public void SameModuleTwice_IsRejected()
    {
        var registered = new[] { Pair("Alpha"), Pair("Alpha") };

        Problems(registered).ShouldContain(p => p.Contains("Alpha 注册了 2 次", StringComparison.Ordinal));
    }

    [Fact]
    public void NonProductionModule_IsSkippedInProduction_AndKeptElsewhere()
    {
        var registered = new[] { Pair("Alpha"), Pair("Beta", onlyIn: KmicEnvironments.NonProduction) };

        Select(registered, "Production").Select(m => m.Module.Name).ShouldBe(["Alpha"]);
        Select(registered, "Development").Select(m => m.Module.Name).ShouldBe(["Alpha", "Beta"]);
    }

    [Fact]
    public void ReferenceModuleWithoutTheEnvironmentRestriction_IsRejectedInProduction()
    {
        var registered = new[] { Pair("Sample") };

        Problems(registered, "Production").ShouldContain(p => p.Contains("生产环境不能注册参考模块 Sample", StringComparison.Ordinal));
        Should.NotThrow(() => Select(registered, "Development"));
    }

    [Fact]
    public void ModuleSwitchedOffInConfiguration_IsLeftOut()
    {
        var registered = new[] { Pair("Alpha"), Pair("Beta") };
        var configuration = Config(("Modules:Beta:Enabled", "false"));

        Select(registered, "Production", configuration).Select(m => m.Module.Name).ShouldBe(["Alpha"]);
    }

    [Fact]
    public void DependencyThatIsRegisteredAndEnabled_IsAccepted()
    {
        var registered = new[] { Pair("Alpha"), Pair("Beta", dependsOn: "Alpha") };

        Should.NotThrow(() => Select(registered, "Production"));
    }

    [Fact]
    public void DependencyThatIsNotRegistered_IsRejected()
    {
        var registered = new[] { Pair("Beta", dependsOn: "Alpha") };

        Problems(registered).ShouldContain(p => p.Contains("依赖的模块 Alpha 没有注册", StringComparison.Ordinal));
    }

    [Fact]
    public void DependencyThatIsSwitchedOff_IsRejected()
    {
        var registered = new[] { Pair("Alpha"), Pair("Beta", dependsOn: "Alpha") };
        var configuration = Config(("Modules:Alpha:Enabled", "false"));

        Problems(registered, "Production", configuration).ShouldContain(p => p.Contains("依赖的模块 Alpha 没有启用", StringComparison.Ordinal));
    }

    [Fact]
    public void DisabledModule_DoesNotNeedItsOwnDependencies()
    {
        // 被关掉的模块不参与依赖检查：它依赖的模块有没有启用都不影响
        var registered = new[] { Pair("Beta", dependsOn: "Missing"), Pair("Alpha") };
        var configuration = Config(("Modules:Beta:Enabled", "false"));

        Should.NotThrow(() => Select(registered, "Production", configuration));
    }

    [Fact]
    public void AllProblemsAreReportedTogether()
    {
        var registered = new[] { Pair("Alpha", endpoints: "Other"), Pair("Beta", dependsOn: "Missing"), Pair("Beta") };

        var thrown = Should.Throw<DeclarationException>(() => Select(registered, "Production"));

        thrown.Problems.Count.ShouldBe(3);
    }

    [Fact]
    public void AssemblyNamesAreChecked_WhenTheSwitchIsOn()
    {
        var registered = new[] { Pair("Alpha") };

        var thrown = Should.Throw<DeclarationException>(() => ModuleSelection.Select(registered, new FakeEnvironment("Production"), Config(), checkAssemblyNames: true));

        thrown.Problems.ShouldContain(p => p.Contains("Application 程序集应当叫 Kmic.Alpha.Application", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("应当在 Kmic.Alpha.Presentation 程序集里", StringComparison.Ordinal));
    }

    private static HostModule Pair(string name, string? endpoints = null, KmicEnvironments onlyIn = KmicEnvironments.Any, string? dependsOn = null) =>
        new(new FakeModule(name, dependsOn), new FakeEndpoints(endpoints ?? name), onlyIn);

    private static List<string> Problems(IReadOnlyList<HostModule> registered, string environment = "Production", IConfiguration? configuration = null) =>
        [.. Should.Throw<DeclarationException>(() => Select(registered, environment, configuration)).Problems];

    private static IReadOnlyList<HostModule> Select(IReadOnlyList<HostModule> registered, string environment, IConfiguration? configuration = null) =>
        ModuleSelection.Select(registered, new FakeEnvironment(environment), configuration ?? Config(), checkAssemblyNames: false);

    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value, StringComparer.Ordinal)).Build();

    private sealed class FakeModule(string name, string? dependsOn) : IModule
    {
        public string Name => name;

        public Assembly ApplicationAssembly => typeof(FakeModule).Assembly;

        public void AddServices(IServiceCollection services)
        {
        }

        public void DeclareDependencies(IModuleDependencies dependencies)
        {
            if (dependsOn is not null)
            {
                dependencies.DependsOn(dependsOn);
            }
        }
    }

    private sealed class FakeEndpoints(string module) : ModuleEndpoints
    {
        public override string Module => module;
    }

    private sealed class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;

        public string ApplicationName { get; set; } = "Test";

        public string ContentRootPath { get; set; } = "";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
