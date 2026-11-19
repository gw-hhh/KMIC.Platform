using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 查询本人未作废、未到期的登录会话，不含模拟会话
/// </summary>
[AllowAuthenticated]
public sealed record ListMySessionsQuery : IQuery<IReadOnlyList<MySessionItem>>;
