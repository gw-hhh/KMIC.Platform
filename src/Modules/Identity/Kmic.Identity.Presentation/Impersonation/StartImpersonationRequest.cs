using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Impersonation;

/// <summary>
/// 开始模拟身份的请求
/// </summary>
/// <param name="RoleId">以他的哪个角色模拟；不给就取他上次使用的角色，没有时取排在最前的启用角色</param>
/// <param name="Reason">原因，必填，最多 500 个字符，写入安全事件</param>
public sealed record StartImpersonationRequest(RoleId? RoleId, string Reason);
