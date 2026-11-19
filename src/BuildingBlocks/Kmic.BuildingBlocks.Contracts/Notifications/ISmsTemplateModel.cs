namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 发短信的模板模型，说明怎样变成短信参数
/// </summary>
public interface ISmsTemplateModel
{
    /// <summary>
    /// 短信服务模板里的参数
    /// </summary>
    IReadOnlyDictionary<string, string> ToSmsParameters();
}
