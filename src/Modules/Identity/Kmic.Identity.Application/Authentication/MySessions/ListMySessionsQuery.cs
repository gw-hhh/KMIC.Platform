using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 查询当前用户的有效登录会话
/// </summary>
/// <remarks>
/// 不包含模拟会话
/// </remarks>
[AllowAuthenticated]
public sealed record ListMySessionsQuery : IQuery<IReadOnlyList<MySessionItem>>;
