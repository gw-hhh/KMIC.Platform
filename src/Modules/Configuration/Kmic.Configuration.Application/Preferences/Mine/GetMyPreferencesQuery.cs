using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 本人在当前角色下全部偏好的生效值
/// </summary>
/// <remarks>
/// 目录里声明的偏好都在，再加上偏好族里已经有值的键；每项带着值来自哪一层。前端启动时拉一次。
/// 模拟登录时返回的是被模拟用户的偏好
/// </remarks>
[AllowAuthenticated]
public sealed record GetMyPreferencesQuery : IQuery<MyPreferencesResponse>;
