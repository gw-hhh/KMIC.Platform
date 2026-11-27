using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>查询完整组织树（含已停用单元，已登录用户均可查询）</summary>
[AllowAuthenticated]
public sealed record GetOrgTreeQuery : IQuery<OrgTreeResponse>;
