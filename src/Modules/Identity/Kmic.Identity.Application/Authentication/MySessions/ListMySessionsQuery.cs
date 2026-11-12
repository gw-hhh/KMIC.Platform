using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 查询自己当前有效的登录设备
/// </summary>
/// <remarks>
/// 不含别人以自己的身份开始的模拟会话：那是管理员排查问题用的，不是自己的登录
/// </remarks>
[AllowAuthenticated]
public sealed record ListMySessionsQuery : IQuery<IReadOnlyList<MySessionItem>>;
