using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>
/// 查询当前用户及授权信息
/// </summary>
/// <remarks>
/// 必须先改密码的用户也能调用
/// </remarks>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
public sealed record GetMeQuery : IQuery<MeResponse>;
