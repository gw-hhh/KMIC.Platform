using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 移除当前角色下的个人偏好设置
/// </summary>
[AllowAuthenticated]
[NotAudited("个人体验配置，不是业务数据")]
[NotIdempotent("未保存个人设置时直接返回成功，无需缓存响应")]
public sealed record ResetMyPreferenceCommand(string Key) : ICommand;
