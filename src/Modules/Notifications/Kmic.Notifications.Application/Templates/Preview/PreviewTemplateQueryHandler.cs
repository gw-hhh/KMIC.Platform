using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Domain.Errors;

namespace Kmic.Notifications.Application.Templates.Preview;

/// <summary>
/// 处理 <see cref="PreviewTemplateQuery"/>
/// </summary>
public sealed class PreviewTemplateQueryHandler(NotificationTemplateCatalog catalog, ITemplateRenderer renderer) : IQueryHandler<PreviewTemplateQuery, TemplatePreviewResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<TemplatePreviewResponse>> Handle(PreviewTemplateQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var definition = catalog.Find(query.Key);
        if (definition is null)
        {
            return NotificationsErrors.TemplateNotFound;
        }

        if (!TemplateChannels.From(definition.Channels).Contains(query.Channel))
        {
            return NotificationsErrors.TemplateChannelNotDeclared;
        }

        object model = definition.Example;
        if (query.Model is { } json)
        {
            if (definition.CarriesSecret)
            {
                return NotificationsErrors.PreviewModelNotAllowed;
            }

            var read = TemplateModels.Read(definition, json);
            if (read.IsFailure)
            {
                return read.Error;
            }

            model = read.Value;
        }

        var rendered = await ChannelRendering.RenderAsync(renderer, query.Channel, query.Subject, query.Body, model, cancellationToken);
        return rendered.IsSuccess
            ? new TemplatePreviewResponse(rendered.Value.Subject, rendered.Value.Body, null)
            : new TemplatePreviewResponse(null, null, rendered.Error.Message);
    }
}
