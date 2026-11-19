namespace Kmic.Identity.Contracts;

/// <summary>
/// 供通知模块查询收件人联系方式
/// </summary>
/// <remarks>
/// 只允许系统身份调用
/// </remarks>
public interface IUserContacts
{
    /// <summary>
    /// 批量查询用户联系方式，不存在的用户不返回
    /// </summary>
    Task<IReadOnlyDictionary<Guid, UserContact>> GetAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);
}
