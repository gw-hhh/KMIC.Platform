using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Kmic.BuildingBlocks.Application.Serialization;

/// <summary>全系统共用的 JSON 序列化选项。</summary>
public static class KmicJsonOptions
{
    private const int MaxDepth = 32;

    // 中文原样输出，HTML 敏感字符保持转义；共用同一实例以确保 Configure 与 Create 行为一致
    private static readonly JavaScriptEncoder _encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

    /// <summary>只读共用默认选项。</summary>
    public static JsonSerializerOptions Default { get; } = ReadOnly(Create());

    /// <summary>读写持久化 JSON 数据的只读选项。</summary>
    public static JsonSerializerOptions Stored { get; } = ReadOnly(CreateForStored());

    /// <summary>创建可修改的选项副本。</summary>
    public static JsonSerializerOptions Create()
    {
        // Strict 模式已禁止未知字段、重复属性及字符串形式数字，并启用可空注解与必填构造参数校验
        var options = new JsonSerializerOptions(JsonSerializerOptions.Strict);
        Apply(options);
        return options;
    }

    /// <summary>将统一序列化配置应用到已有选项实例。</summary>
    public static void Configure(JsonSerializerOptions target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var strict = JsonSerializerOptions.Strict;
        target.UnmappedMemberHandling = strict.UnmappedMemberHandling;
        target.AllowDuplicateProperties = strict.AllowDuplicateProperties;
        target.RespectNullableAnnotations = strict.RespectNullableAnnotations;
        target.RespectRequiredConstructorParameters = strict.RespectRequiredConstructorParameters;
        target.NumberHandling = strict.NumberHandling;
        target.PropertyNameCaseInsensitive = strict.PropertyNameCaseInsensitive;
        target.AllowTrailingCommas = strict.AllowTrailingCommas;
        target.ReadCommentHandling = strict.ReadCommentHandling;
        Apply(target);
    }

    private static JsonSerializerOptions CreateForStored()
    {
        var options = Create();
        options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
        return options;
    }

    private static void Apply(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.MaxDepth = MaxDepth;
        options.Encoder = _encoder;

        // 普通枚举序列化为成员名称，不接受数值输入
        options.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new UtcDateTimeOffsetJsonConverter());
    }

    private static JsonSerializerOptions ReadOnly(JsonSerializerOptions options)
    {
        options.MakeReadOnly();
        return options;
    }
}
