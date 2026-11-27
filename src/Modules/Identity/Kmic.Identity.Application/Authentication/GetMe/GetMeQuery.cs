using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>查询当前用户及授权信息（处于强制改密状态亦可调用）</summary>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
public sealed record GetMeQuery : IQuery<MeResponse>;
