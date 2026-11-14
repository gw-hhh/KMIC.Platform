using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Impersonation;

/// <summary>
/// 开始模拟身份的请求
/// </summary>
/// <param name="RoleId">目标用户的角色 ID；未指定时优先使用上次角色，不可用时选择排序最前的启用角色</param>
/// <param name="Reason">原因，必填，最多 500 个字符，写入安全事件</param>
public sealed record StartImpersonationRequest(RoleId? RoleId, string Reason);
