using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Revert;

/// <summary>
/// 处理 <see cref="RevertTemplateCommand"/>
/// </summary>
public sealed class RevertTemplateCommandHandler(ITemplateRepository templates) : ICommandHandler<RevertTemplateCommand, VersionedResponse<NotificationTemplateId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<NotificationTemplateId>>> Handle(RevertTemplateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await templates.GetByKeyForUpdateAsync(command.Key, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var reverted = loaded.Value.RevertToDefault(command.Channel);
        return reverted.IsFailure ? reverted.Error : new VersionedResponse<NotificationTemplateId>(loaded.Value.Id);
    }
}
