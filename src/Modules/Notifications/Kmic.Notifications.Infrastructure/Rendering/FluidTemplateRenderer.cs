using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Fluid;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Infrastructure.Rendering;

/// <summary>
/// <see cref="ITemplateRenderer" /> 的实现：Fluid（Liquid 语法）
/// </summary>
/// <remarks>
/// 沙箱：成员访问只登记数据模型自己的类型，别的类型和方法都访问不到；不设文件提供器，include、render 读不到任何文件；
/// 步数、用时、输出大小都有上限（Fluid 只有步数上限，用时与输出大小由写入器检查）。HTML 渲染对变量的值转义
/// </remarks>
internal sealed class FluidTemplateRenderer(IOptions<NotificationsRenderOptions> options, TimeProvider time) : ITemplateRenderer
{
    private static readonly FluidParser _parser = new();

    // 只转义 HTML 的特殊字符；默认的编码器会把中文也转成 &#x....;，邮件里的汉字就成了乱码一样的实体
    private static readonly HtmlEncoder _html = HtmlEncoder.Create(UnicodeRanges.All);

    public Result Check(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return _parser.TryParse(template, out _, out var error) ? Result.Success() : NotificationsErrors.TemplateInvalid(error);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "模板来自管理员，渲染过程中模板能触发的任何异常都按“模板有误”处理，不能让它变成未处理的错误")]
    public async ValueTask<Result<string>> RenderAsync(string template, object model, RenderFormat format, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(model);

        if (!_parser.TryParse(template, out var parsed, out var error))
        {
            return NotificationsErrors.TemplateInvalid(error);
        }

        var limits = options.Value;
        var templateOptions = new TemplateOptions { MaxSteps = limits.MaxSteps };
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

    // 模板能访问的只有数据模型沿着公开属性能走到的类型：模型本身、嵌套的对象、集合里的元素。别的类型一概访问不到
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
