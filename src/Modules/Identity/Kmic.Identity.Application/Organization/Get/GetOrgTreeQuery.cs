using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 查询整棵组织树，含已停用的单元
/// </summary>
/// <remarks>
/// 业务页面也要用，登录即可查，不需要组织管理权限
/// </remarks>
[AllowAuthenticated]
public sealed record GetOrgTreeQuery : IQuery<OrgTreeResponse>;
