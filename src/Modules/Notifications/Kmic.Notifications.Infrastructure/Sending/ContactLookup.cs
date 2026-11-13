using Kmic.Identity.Contracts;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 发送时取收件人最新的联系方式：投递记录里不存用户的地址，发送时才向 Identity 取
/// </summary>
internal sealed class ContactLookup(IUserContacts contacts)
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct)
    {
        var found = await contacts.GetAsync([userId], ct);
        return found.GetValueOrDefault(userId);
    }
}
