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
}
