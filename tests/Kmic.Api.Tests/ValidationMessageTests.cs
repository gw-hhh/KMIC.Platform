using System.Reflection;
using FluentValidation;
using FluentValidation.Internal;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Api.Tests;

/// <summary>
/// 校验提示的属性名都是中文：用默认模板的规则，属性要能取到中文名（来自 summary 或 param 的第一个短语）
/// </summary>
/// <remarks>
/// 只查真正会把属性名放进提示的规则：写了 WithName 或 WithMessage 的不查，不要求所有属性都写注释
/// </remarks>
public sealed class ValidationMessageTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    // 子校验器的提示由子校验器自己的规则给出，它本身会被单独检查
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
                        problems.Add($"{validatorType.Name}.{rule.PropertyName}：Must / Custom 没写 WithMessage，默认提示说不清哪里不对");
                    }
                    else if (!rule.GetDisplayName(null).Any(IsChinese))
                    {
                        problems.Add($"{validatorType.Name}.{rule.PropertyName}：取不到中文名，给属性写 summary（记录类型写 param），或写 WithName");
                    }
                }
            }
        }

        problems.ShouldBeEmpty();
    }

    // 写了 WithMessage 的，未格式化的提示就不是语言包里的默认模板
    private static bool UsesDefaultMessage(IRuleComponent component) =>
        string.Equals(component.GetUnformattedErrorMessage(), ValidatorOptions.Global.LanguageManager.GetString(component.Validator.Name), StringComparison.Ordinal);

    private static IEnumerable<Type> ValidatorTypes() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name is { } name && name.StartsWith("Kmic.", StringComparison.Ordinal) && name.EndsWith(".Application", StringComparison.Ordinal))
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && typeof(IValidator).IsAssignableFrom(t) && IsMessageValidator(t));

    // 只查校验命令、查询和它们里面的数据的校验器；框架自己的辅助校验器不在此列
    private static bool IsMessageValidator(Type type) => type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is null;

    private static bool IsChinese(char c) => c is >= '一' and <= '鿿';
}
