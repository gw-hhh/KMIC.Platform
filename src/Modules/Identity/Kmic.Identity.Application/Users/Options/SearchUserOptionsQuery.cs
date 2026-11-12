using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Users.Options;

/// <summary>
/// 选人下拉：按关键字分页找用户
/// </summary>
/// <remarks>
/// 登录即可用，业务页面选负责人、协作者不需要为此单独授权。只返回姓名、工号、所属组织和状态，不含手机号和邮箱
/// </remarks>
/// <param name="Q">关键字，在姓名、英文名、工号、账号里模糊匹配</param>
/// <param name="Page">分页</param>
[AllowAuthenticated]
public sealed record SearchUserOptionsQuery(string? Q, PageRequest Page) : IQuery<PagedResult<UserOption>>;
