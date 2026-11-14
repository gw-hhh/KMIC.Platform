using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.Rendering;

/// <summary>
/// 模板渲染：把模板文本和数据模型渲染成最终内容
/// </summary>
/// <remarks>
/// 模板只能访问数据模型里的公开属性，不能读文件、不能调用任意方法；单次渲染有时间和输出大小的上限，
/// 超过上限当作模板有误。模板来自管理员，不信任
/// </remarks>
public interface ITemplateRenderer
{
    /// <summary>
    /// 检查模板的语法，不渲染
    /// </summary>
    /// <param name="template">模板文本</param>
    Result Check(string template);

    /// <summary>
    /// 渲染
    /// </summary>
    /// <param name="template">模板文本</param>
    /// <param name="model">数据模型，模板里能用的变量就是它的公开属性</param>
    /// <param name="format">用途，决定变量的值是否转义</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>渲染结果；语法错误、超时、输出过大返回“模板有误”，说明原因</returns>
    ValueTask<Result<string>> RenderAsync(string template, object model, RenderFormat format, CancellationToken ct);

    /// <summary>
    /// 严格渲染：和 <see cref="RenderAsync"/> 一样，但模板里用到了数据模型没有的变量就算有误
    /// </summary>
    /// <remarks>
    /// 发布模板时用示例数据严格渲染一遍，写错的变量名在发布时就能发现。发通知时仍然宽松渲染，没有的变量按空处理，
    /// 不会因为一个变量名让整条通知发不出去。只能发现这次渲染实际走到的变量：条件分支没走到的地方查不出来
    /// </remarks>
    /// <param name="template">模板文本</param>
    /// <param name="model">数据模型，模板里能用的变量就是它的公开属性</param>
    /// <param name="format">用途，决定变量的值是否转义</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>渲染结果；除了 <see cref="RenderAsync"/> 的失败情形，用到了没有的变量也返回“模板有误”，并列出变量名</returns>
    ValueTask<Result<string>> RenderStrictAsync(string template, object model, RenderFormat format, CancellationToken ct);
}
