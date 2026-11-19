using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Users.Options;

/// <summary>
/// 选人下拉：按关键字分页找用户
/// </summary>
/// <remarks>
/// 登录即可用，不返回手机号和邮箱
/// </remarks>
/// <param name="Q">关键字</param>
/// <param name="Page">分页</param>
[AllowAuthenticated]
public sealed record SearchUserOptionsQuery(string? Q, PageRequest Page) : IQuery<PagedResult<UserOption>>;
