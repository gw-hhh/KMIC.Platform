using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>
/// 查询当前用户及授权信息
/// </summary>
/// <remarks>
/// 前端启动时和发现授权版本变化时调用。必须先改密码的用户也要能调用，界面据此引导去改密码
/// </remarks>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
public sealed record GetMeQuery : IQuery<MeResponse>;
