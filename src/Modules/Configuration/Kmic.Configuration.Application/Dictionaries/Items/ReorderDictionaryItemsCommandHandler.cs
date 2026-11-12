using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 处理 <see cref="ReorderDictionaryItemsCommand"/>
/// </summary>
public sealed class ReorderDictionaryItemsCommandHandler(IDictionaryTypeRepository types, IAfterCommit afterCommit)
    : ICommandHandler<ReorderDictionaryItemsCommand, VersionedResponse<DictionaryTypeId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<DictionaryTypeId>>> Handle(ReorderDictionaryItemsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await types.GetByCodeForUpdateAsync(command.TypeCode, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var reordered = loaded.Value.ReorderItems(command.ParentCode, command.OrderedCodes);
        if (reordered.IsFailure)
        {
            return reordered.Error;
        }

        DictionaryChanges.Invalidate(afterCommit, command.TypeCode);
        return new VersionedResponse<DictionaryTypeId>(loaded.Value.Id);
    }
}
