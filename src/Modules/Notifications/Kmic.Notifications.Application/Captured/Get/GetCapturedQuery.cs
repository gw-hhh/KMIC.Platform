using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Captured;

namespace Kmic.Notifications.Application.Captured.Get;

/// <summary>
/// 查询一条捕获的通知内容，包含正文
/// </summary>
/// <remarks>
/// 没有开启捕获（生产环境）时返回找不到。内容可能含验证码，查询会记读审计
/// </remarks>
/// <param name="Id">捕获内容</param>
[RequirePermission(NotificationsPermissions.CapturedView)]
[AuditedRead]
public sealed record GetCapturedQuery(CapturedMessageId Id) : IQuery<CapturedDetailResponse>;
