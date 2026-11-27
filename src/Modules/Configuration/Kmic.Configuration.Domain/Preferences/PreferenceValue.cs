using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Domain.Preferences;

/// <summary>分层保存的偏好值</summary>
public sealed class PreferenceValue : AggregateRoot<PreferenceValueId>
{
    /// <summary>键的最大长度，与偏好族键长上限一致</summary>
    public const int MaxKeyLength = 128;

    private PreferenceValue(PreferenceValueId id, string key, PreferenceLayer layer, RoleId? roleId, UserId? userId, string value)
        : base(id)
    {
        Key = key;
        Layer = layer;
        RoleId = roleId;
        UserId = userId;
        Value = value;
    }

    /// <summary>
    /// 偏好键
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// 所在的层
    /// </summary>
    public PreferenceLayer Layer { get; private set; }

    /// <summary>
    /// 角色层和用户层所属的角色；系统层为空
    /// </summary>
    public RoleId? RoleId { get; private set; }

    /// <summary>
    /// 用户层所属的用户；系统层和角色层为空
    /// </summary>
    public UserId? UserId { get; private set; }

    /// <summary>
    /// 值，JSON 对象
    /// </summary>
    public string Value { get; private set; }

    public static PreferenceValue ForSystem(string key, string value) =>
        Create(key, PreferenceLayer.System, null, null, value);

    public static PreferenceValue ForRole(string key, RoleId roleId, string value) =>
        Create(key, PreferenceLayer.Role, roleId, null, value);

    public static PreferenceValue ForUserRole(string key, RoleId roleId, UserId userId, string value) =>
        Create(key, PreferenceLayer.UserRole, roleId, userId, value);

    public void Set(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    private static PreferenceValue Create(string key, PreferenceLayer layer, RoleId? roleId, UserId? userId, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (key.Length > MaxKeyLength)
        {
            throw new ArgumentOutOfRangeException(nameof(key), key.Length, $"偏好键最长 {MaxKeyLength} 个字符");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new PreferenceValue(PreferenceValueId.New(), key, layer, roleId, userId, value);
    }
}
