using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.Rendering;

/// <summary>
/// 根据模板和数据模型渲染通知内容
/// </summary>
/// <remarks>
/// 模板按不可信输入处理
/// </remarks>
public interface ITemplateRenderer
{
    /// <summary>
    /// 仅检查模板语法
    /// </summary>
    Result Check(string template);

    /// <summary>
    /// 渲染通知内容，未定义变量按空值处理
    /// </summary>
    ValueTask<Result<string>> RenderAsync(string template, object model, RenderFormat format, CancellationToken ct);

    /// <summary>
    /// 渲染通知内容，遇到未定义变量时返回模板错误
    /// </summary>
    /// <remarks>
    /// 没执行到的分支查不出
    /// </remarks>
    ValueTask<Result<string>> RenderStrictAsync(string template, object model, RenderFormat format, CancellationToken ct);
}
