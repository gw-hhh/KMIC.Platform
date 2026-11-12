using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 处理 <see cref="DisableDictionaryItemCommand"/>
/// </summary>
public sealed class DisableDictionaryItemCommandHandler(IDictionaryTypeRepository types, IAfterCommit afterCommit)
    : ICommandHandler<DisableDictionaryItemCommand, VersionedResponse<DictionaryTypeId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<DictionaryTypeId>>> Handle(DisableDictionaryItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await types.GetByCodeForUpdateAsync(command.TypeCode, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var disabled = loaded.Value.DisableItem(command.Code);
        if (disabled.IsFailure)
        {
            return disabled.Error;
        }

        DictionaryChanges.Invalidate(afterCommit, command.TypeCode);
        return new VersionedResponse<DictionaryTypeId>(loaded.Value.Id);
    }
}
