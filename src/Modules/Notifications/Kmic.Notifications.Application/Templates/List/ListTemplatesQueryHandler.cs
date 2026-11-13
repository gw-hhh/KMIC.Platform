using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Application.Templates.List;

/// <summary>
/// 处理 <see cref="ListTemplatesQuery"/>
/// </summary>
public sealed class ListTemplatesQueryHandler(INotificationsQueryDb db) : IQueryHandler<ListTemplatesQuery, TemplateListResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<TemplateListResponse>> Handle(ListTemplatesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.Templates
            .OrderBy(t => t.Key)
            .Select(t => new { Template = t, LastPublishedAt = db.TemplateVersions.Where(v => v.TemplateId == t.Id).Max(v => (DateTimeOffset?)v.PublishedAt) })
            .ToListAsync(cancellationToken);
        return new TemplateListResponse([.. rows.Select(r => new TemplateListItem(
            r.Template.Key,
            r.Template.DisplayName,
            r.Template.OwnerModule,
            [.. r.Template.ChannelList.Select(c => c.Code)],
            r.Template.CarriesSecret,
            r.Template.Status.Code,
            NotificationTemplate.ParseActiveVersions(r.Template.ActiveVersions),
            r.LastPublishedAt))]);
    }
}
