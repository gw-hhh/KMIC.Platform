using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>通知模板仓储</summary>
public interface ITemplateRepository : IRepository<NotificationTemplate, NotificationTemplateId>
{
    /// <summary>按模板键加载待修改的模板及其已发布版本</summary>
    Task<Result<NotificationTemplate>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct);

    /// <summary>查询全部模板（不加载版本列表），供目录同步使用</summary>
    Task<IReadOnlyList<NotificationTemplate>> ListAllAsync(CancellationToken ct);

    /// <summary>查询模板各渠道当前选用的发布版本（使用代码默认内容的渠道不在结果中）</summary>
    Task<IReadOnlyDictionary<TemplateChannel, TemplateVersion>> GetActiveVersionsAsync(string key, CancellationToken ct);
}
