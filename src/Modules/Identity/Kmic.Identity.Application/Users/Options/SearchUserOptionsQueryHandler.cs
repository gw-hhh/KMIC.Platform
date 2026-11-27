using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Options;

public sealed class SearchUserOptionsQueryHandler(IIdentityQueryDb db) : IQueryHandler<SearchUserOptionsQuery, PagedResult<UserOption>>
{
    public async ValueTask<Result<PagedResult<UserOption>>> Handle(SearchUserOptionsQuery query, CancellationToken cancellationToken)
    {
        var users = db.Users.Where(u => u.Kind == UserKind.Normal);
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            users = users.Where(u => KmicFunctions.ContainsText(u.DisplayName, query.Q)
                || KmicFunctions.ContainsText(u.EnglishName, query.Q)
                || KmicFunctions.ContainsText(u.EmployeeNo, query.Q)
                || KmicFunctions.ContainsText(u.Account, query.Q));
        }

        return await users
            .Select(u => new UserOption(
                u.Id,
                u.DisplayName,
                u.EmployeeNo,
                db.OrgTrees.SelectMany(t => t.Units).Where(unit => unit.Id == u.OrgUnitId).Select(unit => unit.Name).FirstOrDefault(),
                u.Status))
            .ToPagedAsync(query.Page, new SortRequest(null), UserOptionSort.Map, cancellationToken);
    }
}
