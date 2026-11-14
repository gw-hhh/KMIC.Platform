using Kmic.Identity.Contracts;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 发送时向 Identity 查询系统用户的最新联系方式，投递记录不保存其原始地址
/// </summary>
internal sealed class ContactLookup(IUserContacts contacts)
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct)
    {
        var found = await contacts.GetAsync([userId], ct);
        return found.GetValueOrDefault(userId);
    }
}
