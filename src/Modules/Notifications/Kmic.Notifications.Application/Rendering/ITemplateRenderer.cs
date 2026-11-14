using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.Rendering;

/// <summary>
/// 根据模板和数据模型渲染通知内容
/// </summary>
/// <remarks>
/// 模板按不可信输入处理，仅可访问数据模型的公开属性，不得读文件或调用任意方法。
/// 单次渲染限制时间、执行步数和输出大小，超过限制时返回模板错误。
/// HTML 模式默认转义变量值，文本模式原样输出
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
    /// 错误说明包含未定义变量名；只检查实际执行到的变量，未进入的条件分支无法检查
    /// </remarks>
    ValueTask<Result<string>> RenderStrictAsync(string template, object model, RenderFormat format, CancellationToken ct);
}
