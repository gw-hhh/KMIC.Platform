using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>保存当前用户在当前角色下的个人偏好</summary>
/// <param name="Key">偏好键</param>
/// <param name="Value">值</param>
[AllowAuthenticated]
[NotAudited("仅调整当前用户在当前角色下的偏好")]
[NotIdempotent("按用户、角色和键覆盖偏好值")]
public sealed record SetMyPreferenceCommand(string Key, JsonElement Value) : ICommand;
