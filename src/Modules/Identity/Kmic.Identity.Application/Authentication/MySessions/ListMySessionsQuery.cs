using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 查询本人的登录会话
/// </summary>
/// <remarks>
/// 仅返回未作废且未到绝对到期时间的会话，不含模拟会话
/// </remarks>
[AllowAuthenticated]
public sealed record ListMySessionsQuery : IQuery<IReadOnlyList<MySessionItem>>;
