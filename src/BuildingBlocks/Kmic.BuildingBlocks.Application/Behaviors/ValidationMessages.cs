using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using FluentValidation;
using FluentValidation.Resources;

namespace Kmic.BuildingBlocks.Application.Behaviors;

// 中文化校验提示配置：属性名取自编译期收集的 DisplayNames，显式指定 WithName 或 WithMessage 时优先使用自定义文案
internal static class ValidationMessages
{
    private const string GeneratedTypeName = "Kmic.Generated.DisplayNames";

    private static readonly ConcurrentDictionary<Assembly, IReadOnlyDictionary<string, string>> _byAssembly = new();

    public static void Configure(ValidatorConfiguration options)
    {
        options.LanguageManager = new ChineseMessages { Culture = CultureInfo.GetCultureInfo("zh-CN") };
        options.DisplayNameResolver = (_, member, _) => DisplayName(member);
    }

    public static string? DisplayName(MemberInfo? member)
    {
        if (member?.DeclaringType is not { } type)
        {
            return null;
        }

        return _byAssembly.GetOrAdd(type.Assembly, Load).TryGetValue($"{type.FullName}.{member.Name}", out var name) ? name : null;
    }

    private static IReadOnlyDictionary<string, string> Load(Assembly assembly) =>
        assembly.GetType(GeneratedTypeName)?.GetProperty("Properties", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as IReadOnlyDictionary<string, string>
        ?? FrozenDictionary<string, string>.Empty;

    private sealed class ChineseMessages : LanguageManager
    {
        public ChineseMessages()
        {
            const string Chinese = "zh-CN";
            AddTranslation(Chinese, "NotNullValidator", "{PropertyName}不能为空");
            AddTranslation(Chinese, "NotEmptyValidator", "{PropertyName}不能为空");
            AddTranslation(Chinese, "EmptyValidator", "{PropertyName}必须为空");
            AddTranslation(Chinese, "NullValidator", "{PropertyName}必须为空");
            AddTranslation(Chinese, "LengthValidator", "{PropertyName}要在 {MinLength} 到 {MaxLength} 个字符之间（现在 {TotalLength} 个）");
            AddTranslation(Chinese, "MinimumLengthValidator", "{PropertyName}至少 {MinLength} 个字符（现在 {TotalLength} 个）");
            AddTranslation(Chinese, "MaximumLengthValidator", "{PropertyName}最多 {MaxLength} 个字符（现在 {TotalLength} 个）");
            AddTranslation(Chinese, "ExactLengthValidator", "{PropertyName}必须是 {MaxLength} 个字符（现在 {TotalLength} 个）");
            AddTranslation(Chinese, "EmailValidator", "{PropertyName}不是有效的邮箱地址");
            AddTranslation(Chinese, "RegularExpressionValidator", "{PropertyName}格式不对");
            AddTranslation(Chinese, "GreaterThanValidator", "{PropertyName}必须大于 {ComparisonValue}");
            AddTranslation(Chinese, "GreaterThanOrEqualValidator", "{PropertyName}不能小于 {ComparisonValue}");
            AddTranslation(Chinese, "LessThanValidator", "{PropertyName}必须小于 {ComparisonValue}");
            AddTranslation(Chinese, "LessThanOrEqualValidator", "{PropertyName}不能大于 {ComparisonValue}");
            AddTranslation(Chinese, "InclusiveBetweenValidator", "{PropertyName}要在 {From} 到 {To} 之间");
            AddTranslation(Chinese, "ExclusiveBetweenValidator", "{PropertyName}要大于 {From} 且小于 {To}");
            AddTranslation(Chinese, "EqualValidator", "{PropertyName}必须等于 {ComparisonValue}");
            AddTranslation(Chinese, "NotEqualValidator", "{PropertyName}不能等于 {ComparisonValue}");
            AddTranslation(Chinese, "EnumValidator", "{PropertyName}的取值不在允许范围内");
            AddTranslation(Chinese, "StringEnumValidator", "{PropertyName}的取值不在允许范围内");
            AddTranslation(Chinese, "PrecisionScaleValidator", "{PropertyName}最多 {ExpectedPrecision} 位数字，其中小数最多 {ExpectedScale} 位");
        }
    }
}
