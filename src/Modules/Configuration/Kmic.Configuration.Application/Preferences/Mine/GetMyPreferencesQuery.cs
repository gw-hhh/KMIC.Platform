using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 查询当前角色下生效的个人偏好
/// </summary>
/// <remarks>
/// 包含已声明的偏好和已有可用值的偏好族键，每项标明生效值的来源。
/// 模拟登录时返回的是被模拟用户的偏好
/// </remarks>
[AllowAuthenticated]
public sealed record GetMyPreferencesQuery : IQuery<MyPreferencesResponse>;
