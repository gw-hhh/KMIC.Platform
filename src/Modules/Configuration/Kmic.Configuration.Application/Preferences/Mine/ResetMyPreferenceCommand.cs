using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 恢复本人在当前角色下的一个偏好：删除本人这一层，值回落到角色层，再回落到系统层和默认值
/// </summary>
/// <remarks>
/// 本来就没有设置过时什么也不做
/// </remarks>
/// <param name="Key">偏好键</param>
[AllowAuthenticated]
[NotAudited("个人体验配置，不是业务数据")]
[NotIdempotent("删除本人这一层，重复执行结果相同")]
public sealed record ResetMyPreferenceCommand(string Key) : ICommand;
