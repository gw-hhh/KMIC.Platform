using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 通知模板的仓储
/// </summary>
public interface ITemplateRepository : IRepository<NotificationTemplate, NotificationTemplateId>
{
    /// <summary>
    /// 按模板键读取，用于修改，连同发布过的版本；传了期望版本号时，不一致返回版本冲突
    /// </summary>
    /// <param name="key">模板键</param>
    /// <param name="expectedVersion">调用方看到的并发版本号</param>
    /// <param name="ct">取消令牌</param>
    Task<Result<NotificationTemplate>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 全部模板，不含版本，用于和代码里的声明对账
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<NotificationTemplate>> ListAllAsync(CancellationToken ct);

    /// <summary>
    /// 一个模板各渠道当前生效的发布版本；用默认内容的渠道不在其中，模板不存在返回空
    /// </summary>
    /// <param name="key">模板键</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyDictionary<TemplateChannel, TemplateVersion>> GetActiveVersionsAsync(string key, CancellationToken ct);
}
