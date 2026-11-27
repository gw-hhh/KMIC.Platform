using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>读取用户的分层偏好（系统层和角色层使用缓存，用户层直接读库）</summary>
public interface IPreferenceLayers
{
    /// <summary>获取用户在当前角色下的三层偏好值（userId 为空时不读用户层，roleId 为空时仅读系统层）</summary>
    Task<LayeredPreferences> GetAsync(UserId? userId, RoleId? roleId, CancellationToken ct);
}
