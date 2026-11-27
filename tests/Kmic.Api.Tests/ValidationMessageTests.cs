using System.Reflection;
using FluentValidation;
using FluentValidation.Internal;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests;

public sealed class ValidationMessageTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    // 子校验器的错误提示由其自身规则负责，且该子校验器会被单独扫描检查
    private static readonly string[] _delegating = ["ChildValidatorAdaptor", "PolymorphicValidator"];

    private static readonly string[] _predicates = ["PredicateValidator", "AsyncPredicateValidator"];

    [Fact]
    public void EveryRuleUsingABuiltInMessage_HasAChinesePropertyName()
    {
        var problems = new List<string>();
        using var scope = Factory.Services.CreateScope();
        foreach (var validatorType in ValidatorTypes())
        {
            var validator = (IValidator)ActivatorUtilities.CreateInstance(scope.ServiceProvider, validatorType);
            foreach (var rule in validator.CreateDescriptor().Rules)
            {
                foreach (var component in rule.Components.Where(c => UsesDefaultMessage(c) && !_delegating.Contains(c.Validator.Name, StringComparer.Ordinal)))
                {
                    if (_predicates.Contains(component.Validator.Name, StringComparer.Ordinal))
                    {
                        problems.Add($"{validatorType.Name}.{rule.PropertyName}：Must / Custom 未配置 WithMessage，默认提示无法明确指出错误原因");
                    }
                    else if (!rule.GetDisplayName(null).Any(IsChinese))
                    {
                        problems.Add($"{validatorType.Name}.{rule.PropertyName}：无法获取中文属性名，请为属性补充 summary 注释（record 类型补充 param 注释）或指定 WithName");
                    }
                }
            }
        }

        problems.ShouldBeEmpty();
    }

    // 配置了 WithMessage 的规则，其未格式化提示不会使用语言包默认模板
    private static bool UsesDefaultMessage(IRuleComponent component) =>
        string.Equals(component.GetUnformattedErrorMessage(), ValidatorOptions.Global.LanguageManager.GetString(component.Validator.Name), StringComparison.Ordinal);

    private static IEnumerable<Type> ValidatorTypes() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name is { } name && name.StartsWith("Kmic.", StringComparison.Ordinal) && name.EndsWith(".Application", StringComparison.Ordinal))
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && typeof(IValidator).IsAssignableFrom(t) && IsMessageValidator(t));

    // 仅检查命令、查询及其内嵌模型的校验器，排除框架自身的辅助校验器
    private static bool IsMessageValidator(Type type) => type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is null;

    private static bool IsChinese(char c) => c is >= '一' and <= '鿿';
}
