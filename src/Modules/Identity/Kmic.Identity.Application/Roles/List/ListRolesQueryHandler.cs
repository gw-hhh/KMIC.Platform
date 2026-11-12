using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.List;

/// <summary>
/// 处理 <see cref="ListRolesQuery"/>
/// </summary>
public sealed class ListRolesQueryHandler(IIdentityQueryDb db) : IQueryHandler<ListRolesQuery, PagedResult<RoleListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<RoleListItem>>> Handle(ListRolesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var roles = db.Roles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            roles = roles.Where(r => KmicFunctions.ContainsText(r.Name, query.Q) || (r.Description != null && KmicFunctions.ContainsText(r.Description, query.Q)));
        }

        var statuses = EnumerationFilter.Parse<RoleStatus>(query.Status, "status");
        if (statuses.IsFailure)
        {
            return statuses.Error;
        }

        var wanted = statuses.Value;
        if (wanted.Count > 0)
        {
            roles = roles.Where(r => wanted.Contains(r.Status));
        }

        return await roles
            .Select(r => new RoleListItem(
                r.Id, r.Name, r.Description, r.Status, r.IsProtected, r.GrantsAllPermissions, r.SortOrder, db.Users.Count(u => u.Grants.Any(g => g.RoleId == r.Id)), r.ConcurrencyVersion))
            .ToPagedAsync(query.Page, query.Sort, RoleSort.Map, cancellationToken);
    }
}
