using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>查询当前角色下生效的个人偏好</summary>
[AllowAuthenticated]
public sealed record GetMyPreferencesQuery : IQuery<MyPreferencesResponse>;
