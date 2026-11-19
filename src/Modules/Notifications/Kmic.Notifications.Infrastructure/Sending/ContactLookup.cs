using Kmic.Identity.Contracts;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 查询通知收件人的联系方式
/// </summary>
internal sealed class ContactLookup(IUserContacts contacts)
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct)
    {
        var found = await contacts.GetAsync([userId], ct);
        return found.GetValueOrDefault(userId);
    }
}
