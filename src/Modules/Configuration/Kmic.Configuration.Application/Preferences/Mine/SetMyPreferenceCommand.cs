using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 保存本人在当前角色下的一个偏好
/// </summary>
/// <remarks>
/// 只能改自己的，登录即可；换了角色是另一份。模拟登录是只读的，这个命令在里面被拒绝。
/// 同一个键再写一次是覆盖，重复执行结果相同，所以不要求幂等键
/// </remarks>
/// <param name="Key">偏好键：目录里声明过的，或属于偏好族</param>
/// <param name="Value">值，JSON 对象</param>
[AllowAuthenticated]
[NotAudited("个人体验配置，不是业务数据")]
[NotIdempotent("按键覆盖，重复执行结果相同")]
public sealed record SetMyPreferenceCommand(string Key, JsonElement Value) : ICommand;
