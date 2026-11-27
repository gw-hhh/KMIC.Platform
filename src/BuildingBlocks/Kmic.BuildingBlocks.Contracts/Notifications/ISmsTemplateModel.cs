namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>短信模板模型接口，用于将模型转换为短信服务商参数。</summary>
public interface ISmsTemplateModel
{
    /// <summary>转换为短信服务模板参数字典。</summary>
    IReadOnlyDictionary<string, string> ToSmsParameters();
}
