using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Captured;

namespace Kmic.Notifications.Application.Captured.Get;

/// <summary>查询捕获内容及正文（未开启捕获时返回 NotFound）</summary>
[RequirePermission(NotificationsPermissions.CapturedView)]
[AuditedRead]
public sealed record GetCapturedQuery(CapturedMessageId Id) : IQuery<CapturedDetailResponse>;
