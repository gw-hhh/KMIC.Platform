namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 发短信的模板模型
/// </summary>
/// <remarks>
/// 短信服务只接受“已审核的模板编码 + 参数”，不能发自由文本，所以短信模板的模型要说明怎样变成参数
/// </remarks>
public interface ISmsTemplateModel
{
    /// <summary>
    /// 短信服务模板里的参数
    /// </summary>
    IReadOnlyDictionary<string, string> ToSmsParameters();
}
