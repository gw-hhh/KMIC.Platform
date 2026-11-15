using Kmic.Identity.Contracts;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 查询通知收件人的联系方式
/// </summary>
/// <remarks>
/// 发送时向 Identity 读取系统内用户的最新联系方式，投递记录不保存其原始地址
/// </remarks>
internal sealed class ContactLookup(IUserContacts contacts)
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct)
    {
        var found = await contacts.GetAsync([userId], ct);
        return found.GetValueOrDefault(userId);
    }
}
