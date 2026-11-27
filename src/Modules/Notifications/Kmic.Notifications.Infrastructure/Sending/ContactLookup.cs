using Kmic.Identity.Contracts;

namespace Kmic.Notifications.Infrastructure.Sending;

internal sealed class ContactLookup(IUserContacts contacts)
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct)
    {
        var found = await contacts.GetAsync([userId], ct);
        return found.GetValueOrDefault(userId);
    }
}
