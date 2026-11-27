using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Users.Options;

/// <summary>按关键字分页检索用户选项供选人下拉框使用（仅需登录即可访问，不返回手机号与邮箱）。</summary>
/// <param name="Q">关键字</param>
/// <param name="Page">分页</param>
[AllowAuthenticated]
public sealed record SearchUserOptionsQuery(string? Q, PageRequest Page) : IQuery<PagedResult<UserOption>>;
