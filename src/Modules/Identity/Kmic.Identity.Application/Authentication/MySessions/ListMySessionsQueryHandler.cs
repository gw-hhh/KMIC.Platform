using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Authentication.MySessions;

public sealed class ListMySessionsQueryHandler(IIdentityQueryDb db, IExecutionContext execution, TimeProvider time) : IQueryHandler<ListMySessionsQuery, IReadOnlyList<MySessionItem>>
{
    public async ValueTask<Result<IReadOnlyList<MySessionItem>>> Handle(ListMySessionsQuery query, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("查询登录会话需要已登录的用户身份。");
        var now = time.GetUtcNow();
        var current = access.SessionId;
        var rows = await db.Sessions
            .Where(s => s.UserId == access.UserId && s.ImpersonatorUserId == null && s.Status == SessionStatus.Active && s.AbsoluteExpiresAt > now)
            .OrderByDescending(s => s.LastRefreshedAt)
            .Select(s => new { s.Id, s.ClientType, s.DeviceName, s.LoginIp, CreatedAt = EF.Property<DateTimeOffset>(s, AuditColumns.CreatedAt), s.LastRefreshedAt })
            .ToListAsync(cancellationToken);
        List<MySessionItem> items = [.. rows.Select(r => new MySessionItem(r.Id, r.ClientType, r.DeviceName, r.LoginIp, r.CreatedAt, r.LastRefreshedAt, r.Id == current))];
        Result<IReadOnlyList<MySessionItem>> result = items;
        return result;
    }
}
