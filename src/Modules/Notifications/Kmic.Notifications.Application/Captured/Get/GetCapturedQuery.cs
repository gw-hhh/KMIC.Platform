using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Captured;

namespace Kmic.Notifications.Application.Captured.Get;

/// <summary>
/// 查询捕获内容及正文
/// </summary>
/// <remarks>
/// 未开启捕获时返回找不到；内容可能含验证码，查询须记录读审计
/// </remarks>
[RequirePermission(NotificationsPermissions.CapturedView)]
[AuditedRead]
public sealed record GetCapturedQuery(CapturedMessageId Id) : IQuery<CapturedDetailResponse>;
