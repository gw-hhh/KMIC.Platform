using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;
using Microsoft.Extensions.Options;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 处理 <see cref="AddDictionaryItemCommand"/>
/// </summary>
public sealed class AddDictionaryItemCommandHandler(IDictionaryTypeRepository types, IOptions<DictionaryOptions> options, IAfterCommit afterCommit)
    : ICommandHandler<AddDictionaryItemCommand, VersionedResponse<DictionaryTypeId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<DictionaryTypeId>>> Handle(AddDictionaryItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await types.GetByCodeForUpdateAsync(command.TypeCode, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var added = loaded.Value.AddItem(command.Code, command.Name, command.ParentCode, command.SortOrder, options.Value.MaxItemsPerType);
        if (added.IsFailure)
        {
            return added.Error;
        }

        DictionaryChanges.Invalidate(afterCommit, command.TypeCode);
        return new VersionedResponse<DictionaryTypeId>(loaded.Value.Id);
    }
}
