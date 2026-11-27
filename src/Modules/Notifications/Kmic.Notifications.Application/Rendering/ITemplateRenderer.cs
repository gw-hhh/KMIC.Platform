using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.Rendering;

/// <summary>模板沙箱渲染器（按不可信输入处理模板内容）</summary>
public interface ITemplateRenderer
{
    /// <summary>仅检查模板语法</summary>
    Result Check(string template);

    /// <summary>渲染模板，未定义变量按空值输出</summary>
    ValueTask<Result<string>> RenderAsync(string template, object model, RenderFormat format, CancellationToken ct);

    /// <summary>严格渲染模板，遇到未定义变量返回错误（未执行到的分支不检查）</summary>
    ValueTask<Result<string>> RenderStrictAsync(string template, object model, RenderFormat format, CancellationToken ct);
}
