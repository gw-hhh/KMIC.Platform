namespace Kmic.Identity.Contracts;

/// <summary>
/// 通知发送时查询收件人联系方式用的接口
/// </summary>
/// <remarks>
/// 联系方式是敏感的个人信息，只存在 Identity 一处，其他模块不保存副本。
/// 只允许系统身份调用（Notifications 处理发送请求时就是系统身份），普通用户的请求调用会被拒绝
/// </remarks>
public interface IUserContacts
{
    /// <summary>
    /// 批量取联系方式，不存在的用户不会出现在结果里
    /// </summary>
    /// <param name="userIds">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyDictionary<Guid, UserContact>> GetAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);
}
