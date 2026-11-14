using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 通知模板：代码里声明的一种通知，以及管理员在各渠道发布过的内容版本
/// </summary>
/// <remarks>
/// 声明（名称、渠道、发件账号）来自代码，每次启动时同步。内容默认取代码里的默认内容；
/// 管理员在某个渠道发布版本后，这个渠道改用发布的版本，恢复默认后又回到代码里的内容
/// </remarks>
public sealed class NotificationTemplate : AggregateRoot<NotificationTemplateId>
{
    /// <summary>
    /// 模板键最大长度
    /// </summary>
    public const int KeyMaxLength = 200;

    /// <summary>
    /// 显示名最大长度
    /// </summary>
    public const int DisplayNameMaxLength = 100;

    /// <summary>
    /// 模块名最大长度
    /// </summary>
    public const int ModuleMaxLength = 50;

    /// <summary>
    /// 发件账号名最大长度
    /// </summary>
    public const int SenderProfileMaxLength = 50;

    /// <summary>
    /// 渠道清单最大长度
    /// </summary>
    public const int ChannelsMaxLength = 50;

    private readonly List<TemplateVersion> _versions = [];

    private NotificationTemplate(NotificationTemplateId id, string key, string displayName, string ownerModule, string channels, string senderProfile, bool carriesSecret, string codeHash)
        : base(id)
    {
        Key = key;
        DisplayName = displayName;
        OwnerModule = ownerModule;
        Channels = channels;
        SenderProfile = senderProfile;
        CarriesSecret = carriesSecret;
        CodeHash = codeHash;
        Status = TemplateStatus.Active;
        ActiveVersions = "{}";
    }

    /// <summary>
    /// 模板键，以声明它的模块名开头，唯一
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// 声明它的模块
    /// </summary>
    public string OwnerModule { get; private set; }

    /// <summary>
    /// 发往的渠道，渠道编码用逗号隔开
    /// </summary>
    public string Channels { get; private set; }

    /// <summary>
    /// 发件账号名，只对邮件渠道有意义
    /// </summary>
    public string SenderProfile { get; private set; }

    /// <summary>
    /// 内容含一次性短时凭据（如验证码）
    /// </summary>
    public bool CarriesSecret { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public TemplateStatus Status { get; private set; }

    /// <summary>
    /// 代码里声明内容的哈希，同步时用来判断有没有变化
    /// </summary>
    public string CodeHash { get; private set; }

    /// <summary>
    /// 各渠道当前生效的版本号，JSON：渠道编码到版本号；没有的渠道用代码里的默认内容
    /// </summary>
    public string ActiveVersions { get; private set; }

    /// <summary>
    /// 发布过的版本
    /// </summary>
    public IReadOnlyList<TemplateVersion> Versions => _versions;

    /// <summary>
    /// 登记一个新声明的模板
    /// </summary>
    public static NotificationTemplate Create(string key, string displayName, string ownerModule, IEnumerable<TemplateChannel> channels, string senderProfile, bool carriesSecret, string codeHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        return new NotificationTemplate(NotificationTemplateId.New(), key, displayName, ownerModule, Join(channels), senderProfile ?? string.Empty, carriesSecret, codeHash);
    }

    /// <summary>
    /// 在读取一个渠道清单字符串时还原成渠道
    /// </summary>
    public IReadOnlyList<TemplateChannel> ChannelList => [.. TemplateChannel.All.Where(Supports)];

    /// <summary>
    /// 是否支持指定渠道
    /// </summary>
    public bool Supports(TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        return Channels.Split(',').Contains(channel.Code, StringComparer.Ordinal);
    }

    /// <summary>
    /// 某个渠道当前生效的版本号，用默认内容时为空
    /// </summary>
    public int? ActiveVersionOf(TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        return ParseActiveVersions(ActiveVersions).TryGetValue(channel.Code, out var version) ? version : null;
    }

    /// <summary>
    /// 把代码里的声明同步过来；之前退役的模板重新出现，恢复启用
    /// </summary>
    public void Synchronize(string displayName, IEnumerable<TemplateChannel> channels, string senderProfile, bool carriesSecret, string codeHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        DisplayName = displayName;
        Channels = Join(channels);
        SenderProfile = senderProfile ?? string.Empty;
        CarriesSecret = carriesSecret;
        CodeHash = codeHash;
        Status = TemplateStatus.Active;

        // 代码里不再发往的渠道，发布过的版本留着，但不再生效
        var active = ParseActiveVersions(ActiveVersions).Where(pair => Channels.Split(',').Contains(pair.Key, StringComparer.Ordinal)).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        ActiveVersions = JsonSerializer.Serialize(active);
    }

    /// <summary>
    /// 将已移除声明的模板标为退役
    /// </summary>
    public void Retire() => Status = TemplateStatus.Retired;

    /// <summary>
    /// 发布新版本并设为该渠道的生效版本
    /// </summary>
    /// <remarks>
    /// 渠道须已声明；主题在站内消息中用作标题
    /// </remarks>
    public Result<TemplateVersion> Publish(TemplateChannel channel, string subject, string body, UserId publishedBy, string? remark, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var allowed = CanChange(channel);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var next = _versions.Count == 0 ? 1 : _versions.Max(v => v.Version) + 1;
        var version = TemplateVersion.Create(Id, next, channel, subject.Trim(), body, publishedBy, now, remark);
        _versions.Add(version);

        var active = ParseActiveVersions(ActiveVersions).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        active[channel.Code] = next;
        ActiveVersions = JsonSerializer.Serialize(active);
        return version;
    }

    /// <summary>
    /// 恢复某个渠道的默认内容；历史版本保留
    /// </summary>
    public Result RevertToDefault(TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        var allowed = CanChange(channel);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var active = ParseActiveVersions(ActiveVersions).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        if (active.Remove(channel.Code))
        {
            ActiveVersions = JsonSerializer.Serialize(active);
        }

        return Result.Success();
    }

    /// <summary>
    /// 读出各渠道生效的版本号
    /// </summary>
    /// <param name="json">ActiveVersions 的内容</param>
    public static IReadOnlyDictionary<string, int> ParseActiveVersions(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];

    private static string Join(IEnumerable<TemplateChannel> channels)
    {
        var set = channels.ToHashSet();
        return string.Join(',', TemplateChannel.All.Where(set.Contains).Select(c => c.Code));
    }

    /// <summary>
    /// 检查渠道内容是否可修改；模板已退役或未声明该渠道时返回错误
    /// </summary>
    public Result CanChange(TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (Status == TemplateStatus.Retired)
        {
            return NotificationsErrors.TemplateRetired;
        }

        return Supports(channel) ? Result.Success() : NotificationsErrors.TemplateChannelNotDeclared;
    }
}
