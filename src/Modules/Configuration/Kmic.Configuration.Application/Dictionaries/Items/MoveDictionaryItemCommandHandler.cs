using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 处理 <see cref="MoveDictionaryItemCommand"/>
/// </summary>
public sealed class MoveDictionaryItemCommandHandler(IDictionaryTypeRepository types, IAfterCommit afterCommit)
    : ICommandHandler<MoveDictionaryItemCommand, VersionedResponse<DictionaryTypeId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<DictionaryTypeId>>> Handle(MoveDictionaryItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await types.GetByCodeForUpdateAsync(command.TypeCode, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var moved = loaded.Value.MoveItem(command.Code, command.NewParentCode);
        if (moved.IsFailure)
        {
            return moved.Error;
        }

        DictionaryChanges.Invalidate(afterCommit, command.TypeCode);
        return new VersionedResponse<DictionaryTypeId>(loaded.Value.Id);
    }
}
