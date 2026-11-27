using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Fluid;
using Fluid.Values;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Infrastructure.Rendering;

internal sealed class FluidTemplateRenderer(IOptions<NotificationsRenderOptions> options, TimeProvider time) : ITemplateRenderer
{
    private static readonly FluidParser _parser = new();

    // 转义 HTML 特殊字符，保留中文，避免生成不必要的字符实体
    private static readonly HtmlEncoder _html = HtmlEncoder.Create(UnicodeRanges.All);

    public Result Check(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return _parser.TryParse(template, out _, out var error) ? Result.Success() : NotificationsErrors.TemplateInvalid(error);
    }

    public ValueTask<Result<string>> RenderAsync(string template, object model, RenderFormat format, CancellationToken ct) =>
        RenderCoreAsync(template, model, format, undefined: null, ct);

    public async ValueTask<Result<string>> RenderStrictAsync(string template, object model, RenderFormat format, CancellationToken ct)
    {
        var undefined = new List<string>();
        var rendered = await RenderCoreAsync(template, model, format, undefined, ct);
        return rendered.IsSuccess && undefined.Count > 0
            ? NotificationsErrors.TemplateInvalid($"未声明的变量：{string.Join("、", undefined.Distinct(StringComparer.Ordinal))}")
            : rendered;
    }

    // 传入 undefined 列表时收集未定义变量名；渲染继续，变量按空值处理
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "将不可信模板触发的渲染异常转换为模板错误，避免异常中断请求")]
    private async ValueTask<Result<string>> RenderCoreAsync(string template, object model, RenderFormat format, List<string>? undefined, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(model);

        if (!_parser.TryParse(template, out var parsed, out var error))
        {
            return NotificationsErrors.TemplateInvalid(error);
        }

        var limits = options.Value;
        var templateOptions = new TemplateOptions { MaxSteps = limits.MaxSteps };
        if (undefined is not null)
        {
            templateOptions.Undefined = name =>
            {
                undefined.Add(name);
                return ValueTask.FromResult<FluidValue>(NilValue.Instance);
            };
        }

        Register(templateOptions, model.GetType(), []);
        var context = new TemplateContext(model, templateOptions);
        using var writer = new CappedWriter(limits.MaxOutputKb * 1024, TimeSpan.FromMilliseconds(limits.RenderTimeoutMs), time);
        try
        {
            await parsed.RenderAsync(writer, format == RenderFormat.Html ? _html : NullEncoder.Default, context);
            return writer.ToString();
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            return NotificationsErrors.TemplateInvalid(exception.Message);
        }
    }

    // 仅注册模型及其公开属性可达的嵌套类型和集合元素类型，限制模板成员访问
    private static void Register(TemplateOptions options, Type type, HashSet<Type> seen)
    {
        var element = ElementOf(type) ?? type;
        if (IsPlain(element) || !seen.Add(element))
        {
            return;
        }

        options.MemberAccessStrategy.Register(element);
        foreach (var property in element.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            Register(options, property.PropertyType, seen);
        }
    }

    private static Type? ElementOf(Type type) =>
        type == typeof(string)
            ? null
            : type.GetInterfaces().Append(type).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GetGenericArguments()[0];

    private static bool IsPlain(Type type)
    {
        var core = Nullable.GetUnderlyingType(type) ?? type;
        return core.IsPrimitive || core.IsEnum || core == typeof(string) || core == typeof(decimal) || core == typeof(Guid) || core == typeof(DateTime)
            || core == typeof(DateTimeOffset) || core == typeof(DateOnly) || core == typeof(TimeOnly) || core == typeof(TimeSpan);
    }
}
