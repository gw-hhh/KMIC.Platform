using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 查询整棵组织树，含已停用的单元
/// </summary>
/// <remarks>
/// 登录即可用：业务页面按组织筛选、显示所属组织不需要为此单独授权。只返回名称、编码、层级这些信息
/// </remarks>
[AllowAuthenticated]
public sealed record GetOrgTreeQuery : IQuery<OrgTreeResponse>;
