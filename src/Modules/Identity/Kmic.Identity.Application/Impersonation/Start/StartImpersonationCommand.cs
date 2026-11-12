using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authentication;

namespace Kmic.Identity.Application.Impersonation.Start;

/// <summary>
/// 开始模拟身份：以某个用户的身份查看系统，用来排查“为什么他看到的和我不一样”
/// </summary>
/// <remarks>
/// 创建一个独立的模拟会话，管理员自己的会话不受影响；模拟会话只签发访问令牌，不签发刷新令牌，有效期就是会话的上限（默认 1 小时）。
/// 默认只读；操作者另有“模拟身份时允许写入”权限点时，会话可以修改数据，开始时确定，期间不变。
/// 不能模拟自己、系统账号和受保护角色的持有人。响应里有访问令牌，所以不做幂等：幂等记录会把响应存进数据库
/// </remarks>
/// <param name="TargetUserId">被模拟的用户，必须启用</param>
/// <param name="RoleId">以他的哪个角色模拟；不给就取他上次使用的角色，没有时取排在最前的启用角色</param>
/// <param name="Reason">原因，必填，写入安全事件</param>
/// <param name="ClientIp">操作者的 IP，由接口层填，不是请求体的一部分</param>
/// <param name="UserAgent">操作者的 User-Agent，由接口层填，不是请求体的一部分</param>
[RequirePermission(IdentityPermissions.UserImpersonate)]
[Audited]
[NotIdempotent("响应里有访问令牌，不能存进幂等记录；重复提交会开始另一个模拟会话")]
public sealed record StartImpersonationCommand(UserId TargetUserId, RoleId? RoleId, string Reason, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>;
