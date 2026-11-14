namespace Kmic.Identity.Contracts;

/// <summary>
/// 供通知模块查询收件人联系方式
/// </summary>
/// <remarks>
/// 联系方式由 Identity 维护，调用方不得另行维护联系人副本。
/// 仅允许系统身份调用，普通用户请求会被拒绝
/// </remarks>
public interface IUserContacts
{
    /// <summary>
    /// 批量查询用户联系方式，不存在的用户不返回
    /// </summary>
    Task<IReadOnlyDictionary<Guid, UserContact>> GetAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);
}
