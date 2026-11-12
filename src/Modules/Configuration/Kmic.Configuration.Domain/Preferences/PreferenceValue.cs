using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Domain.Preferences;

/// <summary>
/// 偏好在某一层上的值：同一个键在同一层只有一行
/// </summary>
/// <remarks>
/// 层与角色、用户列的对应由三个工厂方法保证，数据库另有检查约束。
/// 值的校验不在聚合里：校验函数是偏好类型自己的静态成员，由命令处理器按偏好目录取到并执行，这里只存已经校验过的 JSON
/// </remarks>
public sealed class PreferenceValue : AggregateRoot<PreferenceValueId>
{
    /// <summary>
    /// 键的最大长度，与偏好族的键长上限一致
    /// </summary>
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

    /// <summary>
    /// 系统层的值
    /// </summary>
    public static PreferenceValue ForSystem(string key, string value) =>
        Create(key, PreferenceLayer.System, null, null, value);

    /// <summary>
    /// 角色层的值
    /// </summary>
    public static PreferenceValue ForRole(string key, RoleId roleId, string value) =>
        Create(key, PreferenceLayer.Role, roleId, null, value);

    /// <summary>
    /// 用户在某个角色下的值
    /// </summary>
    public static PreferenceValue ForUserRole(string key, RoleId roleId, UserId userId, string value) =>
        Create(key, PreferenceLayer.UserRole, roleId, userId, value);

    /// <summary>
    /// 换成新的值
    /// </summary>
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
