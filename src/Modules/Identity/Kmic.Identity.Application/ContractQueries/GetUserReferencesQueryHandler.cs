using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

public sealed class GetUserReferencesQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetUserReferencesQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
{
    public async ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(GetUserReferencesQuery query, CancellationToken cancellationToken)
    {
        var rows = await db.Users
            .Where(u => query.Ids.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, u.EmployeeNo, u.Status })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(
            r => r.Id.Value,
            r => new ReferenceInfo(r.Id.Value, r.DisplayName, r.EmployeeNo, r.Status == UserStatus.Active ? ReferenceStatus.Active : ReferenceStatus.Disabled));
    }
}
