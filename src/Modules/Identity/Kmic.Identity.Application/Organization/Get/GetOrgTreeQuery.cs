using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 查询整棵组织树，含已停用的单元
/// </summary>
/// <remarks>
/// 供业务页面筛选组织和显示所属组织，无需组织管理权限
/// </remarks>
[AllowAuthenticated]
public sealed record GetOrgTreeQuery : IQuery<OrgTreeResponse>;
