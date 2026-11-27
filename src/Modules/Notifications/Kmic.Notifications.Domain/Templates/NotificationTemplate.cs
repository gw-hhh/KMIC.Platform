using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>通知模板及其各渠道发布版本</summary>
public sealed class NotificationTemplate : AggregateRoot<NotificationTemplateId>
{
    /// <summary>模板键最大长度</summary>
    public const int KeyMaxLength = 200;

    /// <summary>显示名最大长度</summary>
    public const int DisplayNameMaxLength = 100;

    /// <summary>模块名最大长度</summary>
    public const int ModuleMaxLength = 50;

    /// <summary>发件账号名最大长度</summary>
    public const int SenderProfileMaxLength = 50;

    /// <summary>渠道清单最大长度</summary>
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
    /// 各渠道选用的发布版本号（JSON：渠道编码到版本号）
    /// </summary>
    public string ActiveVersions { get; private set; }

    /// <summary>
    /// 发布过的版本
    /// </summary>
    public IReadOnlyList<TemplateVersion> Versions => _versions;

    /// <summary>登记新声明的模板</summary>
    public static NotificationTemplate Create(string key, string displayName, string ownerModule, IEnumerable<TemplateChannel> channels, string senderProfile, bool carriesSecret, string codeHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        return new NotificationTemplate(NotificationTemplateId.New(), key, displayName, ownerModule, Join(channels), senderProfile ?? string.Empty, carriesSecret, codeHash);
    }

    /// <summary>
    /// 模板支持的渠道列表
    /// </summary>
    public IReadOnlyList<TemplateChannel> ChannelList => [.. TemplateChannel.All.Where(Supports)];

    /// <summary>是否支持指定渠道</summary>
    public bool Supports(TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        return Channels.Split(',').Contains(channel.Code, StringComparer.Ordinal);
    }

    /// <summary>指定渠道选用的发布版本号，未选用时返回 null</summary>
    public int? ActiveVersionOf(TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        return ParseActiveVersions(ActiveVersions).TryGetValue(channel.Code, out var version) ? version : null;
    }

    /// <summary>同步代码中的模板声明（已废弃模板重新出现时恢复启用）</summary>
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

        // 移除已不再支持的渠道启用记录，历史发布版本仍保留
        var active = ParseActiveVersions(ActiveVersions).Where(pair => Channels.Split(',').Contains(pair.Key, StringComparer.Ordinal)).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        ActiveVersions = JsonSerializer.Serialize(active);
    }

    /// <summary>标记为已废弃（代码声明已移除）</summary>
    public void Retire() => Status = TemplateStatus.Retired;

    /// <summary>发布新版本并设为该渠道当前生效版本（站内消息的 <paramref name="subject" /> 即消息标题）</summary>
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

    /// <summary>将指定渠道恢复为代码默认内容，保留历史发布版本</summary>
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

    /// <summary>解析各渠道选用的发布版本号</summary>
    public static IReadOnlyDictionary<string, int> ParseActiveVersions(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];

    private static string Join(IEnumerable<TemplateChannel> channels)
    {
        var set = channels.ToHashSet();
        return string.Join(',', TemplateChannel.All.Where(set.Contains).Select(c => c.Code));
    }

    /// <summary>检查指定渠道是否允许修改</summary>
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
