using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users.List;

public sealed class ListUsersQueryHandler(IIdentityQueryDb db) : IQueryHandler<ListUsersQuery, PagedResult<UserListItem>>
{
    public async ValueTask<Result<PagedResult<UserListItem>>> Handle(ListUsersQuery query, CancellationToken cancellationToken)
    {
        var statuses = EnumerationFilter.Parse<UserStatus>(query.Status, "status");
        if (statuses.IsFailure)
        {
            return statuses.Error;
        }

        // 过滤掉系统操作账号，仅展示普通用户
        var users = db.Users.Where(u => u.Kind == UserKind.Normal);
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            users = users.Where(u => KmicFunctions.ContainsText(u.Account, query.Q)
                || KmicFunctions.ContainsText(u.DisplayName, query.Q)
                || KmicFunctions.ContainsText(u.EnglishName, query.Q)
                || KmicFunctions.ContainsText(u.EmployeeNo, query.Q)
                || KmicFunctions.ContainsText(u.Mobile, query.Q));
        }

        var wanted = statuses.Value;
        if (wanted.Count > 0)
        {
            users = users.Where(u => wanted.Contains(u.Status));
        }

        if (query.RoleId is { } roleId)
        {
            users = users.Where(u => u.Grants.Any(g => g.RoleId == roleId));
        }

        if (query.OrgUnitId is { } orgUnitId)
        {
            users = users.Where(u => u.OrgUnitId == orgUnitId);
        }

        var page = await users
            .Select(u => new UserListItem(u.Id, u.Account, u.DisplayName, u.EmployeeNo, u.Mobile, u.Status, u.OrgUnitId, null, Array.Empty<UserRoleBrief>(), u.ConcurrencyVersion))
            .ToPagedAsync(query.Page, query.Sort, UserSort.Map, cancellationToken);
        return page.IsFailure ? page.Error : await WithNamesAsync(page.Value, cancellationToken);
    }

    // 仅对当前页用户批量补全角色与组织名称，避免在分页排序前联表
    private async Task<PagedResult<UserListItem>> WithNamesAsync(PagedResult<UserListItem> page, CancellationToken ct)
    {
        if (page.Items.Count == 0)
        {
            return page;
        }

        List<UserId> ids = [.. page.Items.Select(i => i.Id)];
        var grants = await db.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, RoleIds = u.Grants.Select(g => g.RoleId).ToList() })
            .ToListAsync(ct);
        var roles = await UserNames.RolesAsync(db, grants.SelectMany(g => g.RoleIds), ct);
        var orgUnits = await UserNames.OrgUnitsAsync(db, page.Items.Where(i => i.OrgUnitId is not null).Select(i => i.OrgUnitId!.Value), ct);
        var roleIdsByUser = grants.ToDictionary(g => g.Id, g => g.RoleIds);

        return page with
        {
            Items =
            [
                .. page.Items.Select(item => item with
                {
                    OrgUnitName = item.OrgUnitId is { } unit ? orgUnits.GetValueOrDefault(unit) : null,
                    Roles =
                    [
                        .. roleIdsByUser.GetValueOrDefault(item.Id, [])
                            .Where(roles.ContainsKey)
                            .Select(id => new UserRoleBrief(id, roles[id].Name, roles[id].IsActive))
                            .OrderBy(r => r.Name, StringComparer.Ordinal),
                    ],
                }),
            ],
        };
    }
}
