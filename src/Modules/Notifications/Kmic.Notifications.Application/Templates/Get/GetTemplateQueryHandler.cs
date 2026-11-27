using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Application.Templates.Get;

public sealed class GetTemplateQueryHandler(INotificationsQueryDb db, NotificationTemplateCatalog catalog) : IQueryHandler<GetTemplateQuery, TemplateDetailResponse>
{
    public async ValueTask<Result<TemplateDetailResponse>> Handle(GetTemplateQuery query, CancellationToken cancellationToken)
    {
        var template = await db.Templates.FirstOrDefaultAsync(t => t.Key == query.Key, cancellationToken);
        if (template is null)
        {
            return NotificationsErrors.TemplateNotFound;
        }

        var versions = await db.TemplateVersions.Where(v => v.TemplateId == template.Id).OrderByDescending(v => v.Version).ToListAsync(cancellationToken);

        // 已从代码中移除声明的模板仅保留数据库记录，无默认内容、变量和示例数据
        var definition = catalog.Find(template.Key);
        var active = NotificationTemplate.ParseActiveVersions(template.ActiveVersions);
        var channels = template.ChannelList.Select(channel => ChannelOf(channel, definition, active, versions)).ToList();
        return new TemplateDetailResponse(
            template.Key,
            template.DisplayName,
            template.OwnerModule,
            template.SenderProfile,
            template.CarriesSecret,
            template.Status.Code,
            template.ConcurrencyVersion,
            channels,
            definition?.Variables ?? [],
            definition is null ? null : JsonSerializer.SerializeToElement(definition.Example, definition.ModelType, NotificationModelJson.Options),
            [.. versions.Select(v => new TemplateVersionInfo(v.Version, v.Channel.Code, v.Subject, v.Body, v.PublishedBy.Value, v.PublishedAt, v.Remark))]);
    }

    private static TemplateChannelInfo ChannelOf(TemplateChannel channel, NotificationTemplateDefinition? definition, IReadOnlyDictionary<string, int> active, List<TemplateVersion> versions)
    {
        var defaultSubject = definition?.DefaultSubject;
        var defaultBody = definition is null ? null : TemplateChannels.DefaultBody(definition, channel);
        var published = active.TryGetValue(channel.Code, out var number)
            ? versions.Find(v => v.Channel == channel && v.Version == number)
            : null;
        return new TemplateChannelInfo(channel.Code, published?.Version, published?.Subject ?? defaultSubject ?? string.Empty, published?.Body ?? defaultBody ?? string.Empty, defaultSubject, defaultBody);
    }
}
