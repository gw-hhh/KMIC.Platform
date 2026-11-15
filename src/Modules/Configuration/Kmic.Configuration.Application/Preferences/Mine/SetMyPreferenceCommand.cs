using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 保存当前角色下的个人偏好
/// </summary>
/// <remarks>
/// 仅修改当前用户在当前角色下的偏好；只读模拟会话不可调用。
/// </remarks>
/// <param name="Key">目录中已声明的偏好键，或已声明偏好族下的键</param>
/// <param name="Value">值，JSON 对象</param>
[AllowAuthenticated]
[NotAudited("仅调整当前用户在当前角色下的偏好")]
[NotIdempotent("按用户、角色和键覆盖偏好值")]
public sealed record SetMyPreferenceCommand(string Key, JsonElement Value) : ICommand;
