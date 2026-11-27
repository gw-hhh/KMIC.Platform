using System.Text.RegularExpressions;
using Kmic.ArchitectureTests.Support;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Shouldly;

namespace Kmic.ArchitectureTests;

public sealed partial class HostRuleTests
{
    // 管道行为的执行顺序，宿主在 AddMediator 中按此顺序注册
    private static readonly string[] _expectedBehaviors =
    [
        "LoggingBehavior", "ExceptionTranslationBehavior", "FeatureGateBehavior", "ValidationBehavior", "AuthorizationBehavior",
        "ReadAuditBehavior", "IdempotencyBehavior", "TransactionBehavior",
    ];

    private static string ProgramText => File.ReadAllText(Path.Combine(ProductAssemblies.RepositoryRoot(), "src", "Hosts", "Kmic.Api", "Program.cs"));

    [GeneratedRegex(@"typeof\((?<name>\w+)<,>\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BehaviorPattern();

    [Fact]
    public void Host_DeclaresThePipelineBehaviorsInTheAgreedOrder() =>
        BehaviorPattern().Matches(ProgramText).Select(m => m.Groups["name"].Value).ShouldBe(_expectedBehaviors);

    [Fact]
    public void EveryModuleRegistrationClass_IsRegisteredInTheHost()
    {
        var modules = ProductAssemblies.Of(Layer.Infrastructure)
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IModule).IsAssignableFrom(t))
            .Select(t => t.Name)
            .ToList();

        modules.ShouldNotBeEmpty();
        foreach (var module in modules)
        {
            ProgramText.ShouldContain($"AddModule<{module},", Case.Sensitive, $"{module} 未在 Kmic.Api 的 Program.cs 中注册。");
        }
    }

    [Fact]
    public void EveryModuleRegistrationClass_IsListedInTheMigrationRunner()
    {
        var program = File.ReadAllText(Path.Combine(ProductAssemblies.RepositoryRoot(), "src", "Hosts", "Kmic.MigrationRunner", "Program.cs"));
        var modules = ProductAssemblies.Of(Layer.Infrastructure)
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IModule).IsAssignableFrom(t))
            .Select(t => t.Name)
            .ToList();

        modules.ShouldNotBeEmpty();
        foreach (var module in modules)
        {
            program.ShouldContain($".Add<{module}>(", Case.Sensitive, $"{module} 未在 Kmic.MigrationRunner 的 Program.cs 中注册。");
        }
    }

    [Fact]
    public void OnlyTheRootHasAnEditorConfig()
    {
        var root = ProductAssemblies.RepositoryRoot();
        var found = Directory.EnumerateFiles(root, ".editorconfig", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(f => Path.GetRelativePath(root, f))
            .Where(f => !string.Equals(f, ".editorconfig", StringComparison.Ordinal));

        found.ShouldBeEmpty();
    }
}
