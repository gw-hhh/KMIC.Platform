using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Kmic.BuildingBlocks.Application.Serialization;

/// <summary>
/// 全系统共用的 JSON 选项
/// </summary>
public static class KmicJsonOptions
{
    private const int MaxDepth = 32;

    // 中文原样输出，HTML 敏感字符仍然转义。共用一个实例，Configure 写进去的和 Create 的才是同一个
    private static readonly JavaScriptEncoder _encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

    /// <summary>
    /// 只读的共用实例
    /// </summary>
    public static JsonSerializerOptions Default { get; } = ReadOnly(Create());

    /// <summary>
    /// 读写已落库数据（集成消息、幂等记录里的响应）用的只读实例
    /// </summary>
    public static JsonSerializerOptions Stored { get; } = ReadOnly(CreateForStored());

    /// <summary>
    /// 新建一份可修改的选项
    /// </summary>
    public static JsonSerializerOptions Create()
    {
        // Strict 已经禁止未知字段、重复属性和字符串形式的数字，也检查可空注解和必填构造参数
        var options = new JsonSerializerOptions(JsonSerializerOptions.Strict);
        Apply(options);
        return options;
    }

    /// <summary>
    /// 把同样的设置写进已有的选项
    /// </summary>
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

        // 普通枚举写成员名，不接受数字
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
