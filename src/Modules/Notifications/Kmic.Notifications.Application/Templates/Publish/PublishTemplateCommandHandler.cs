using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Publish;

/// <summary>
/// 处理 <see cref="PublishTemplateCommand"/>
/// </summary>
public sealed class PublishTemplateCommandHandler(
    ITemplateRepository templates,
    NotificationTemplateCatalog catalog,
    ITemplateRenderer renderer,
    IExecutionContext execution,
    TimeProvider time) : ICommandHandler<PublishTemplateCommand, VersionedResponse<NotificationTemplateId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<NotificationTemplateId>>> Handle(PublishTemplateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await templates.GetByKeyForUpdateAsync(command.Key, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var template = loaded.Value;
        var allowed = template.CanChange(command.Channel);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        // 用示例数据渲染一遍，模板有误在发布时就发现，不等到发通知时才出错
        var definition = catalog.Find(template.Key);
        if (definition is null)
        {
            return NotificationsErrors.TemplateRetired;
        }

        var rendered = await ChannelRendering.RenderAsync(renderer, command.Channel, command.Subject, command.Body, definition.Example, cancellationToken);
        if (rendered.IsFailure)
        {
            return rendered.Error;
        }

        var by = (execution.Access ?? throw new InvalidOperationException("发布模板需要已登录的访问上下文。")).UserId;
        var published = template.Publish(command.Channel, command.Subject, command.Body, by, command.Remark, time.GetUtcNow());
        return published.IsFailure ? published.Error : new VersionedResponse<NotificationTemplateId>(template.Id);
    }
}
