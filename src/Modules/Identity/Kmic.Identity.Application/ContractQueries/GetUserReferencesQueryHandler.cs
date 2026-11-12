using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetUserReferencesQuery"/>
/// </summary>
/// <remarks>
/// 用户不删除，只禁用：别的模块里保存着的负责人、创建人，禁用之后仍然解析得出来，状态是“已停用”。
/// 编码取工号。Identity 的数据不按数据范围过滤，所以不需要绕过声明
/// </remarks>
public sealed class GetUserReferencesQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetUserReferencesQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(GetUserReferencesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.Users
            .Where(u => query.Ids.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, u.EmployeeNo, u.Status })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(
            r => r.Id.Value,
            r => new ReferenceInfo(r.Id.Value, r.DisplayName, r.EmployeeNo, r.Status == UserStatus.Active ? ReferenceStatus.Active : ReferenceStatus.Disabled));
    }
}
