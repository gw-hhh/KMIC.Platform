using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 通知模板的仓储
/// </summary>
public interface ITemplateRepository : IRepository<NotificationTemplate, NotificationTemplateId>
{
    /// <summary>
    /// 按模板键读取待修改的模板及其已发布版本
    /// </summary>
    /// <remarks>
    /// 指定 expectedVersion 时校验并发版本，不一致则返回版本冲突
    /// </remarks>
    Task<Result<NotificationTemplate>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 查询全部模板，不加载版本，供目录同步使用
    /// </summary>
    Task<IReadOnlyList<NotificationTemplate>> ListAllAsync(CancellationToken ct);

    /// <summary>
    /// 查询模板各渠道当前生效的发布版本
    /// </summary>
    /// <remarks>
    /// 使用代码默认内容的渠道不在结果中；模板不存在时返回空集合
    /// </remarks>
    Task<IReadOnlyDictionary<TemplateChannel, TemplateVersion>> GetActiveVersionsAsync(string key, CancellationToken ct);
}
