using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Features;

/// <summary>
/// 当前对请求开放的功能开关
/// </summary>
/// <remarks>
/// 前端启动时拉一次，据此显示或隐藏入口。开关只决定功能是否开放，不授予权限：开启后仍按权限点与数据范围授权
/// </remarks>
[AllowAuthenticated]
public sealed record GetFeaturesQuery : IQuery<FeaturesResponse>;
