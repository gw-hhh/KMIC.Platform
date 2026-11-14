using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 保存本人在当前角色下的偏好
/// </summary>
/// <remarks>
/// 仅可修改本人偏好，按当前角色分别保存。只读模拟会话不可调用。
/// 同一键重复写入会覆盖原值，无需幂等键
/// </remarks>
/// <param name="Key">偏好键：目录里声明过的，或属于偏好族</param>
/// <param name="Value">值，JSON 对象</param>
[AllowAuthenticated]
[NotAudited("个人体验配置，不是业务数据")]
[NotIdempotent("同一键覆盖个人设置，无需缓存响应")]
public sealed record SetMyPreferenceCommand(string Key, JsonElement Value) : ICommand;
