using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetUserContactsQuery"/>
/// </summary>
public sealed class GetUserContactsQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetUserContactsQuery, IReadOnlyDictionary<Guid, UserContact>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyDictionary<Guid, UserContact>>> Handle(GetUserContactsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.Users
            .Where(u => query.Ids.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, u.Email, u.Mobile, u.Status })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.Id.Value, r => new UserContact(r.Id.Value, r.DisplayName, r.Email, r.Mobile, r.Status == UserStatus.Active));
    }
}
