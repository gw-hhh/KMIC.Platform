using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 处理 <see cref="EnableDictionaryItemCommand"/>
/// </summary>
public sealed class EnableDictionaryItemCommandHandler(IDictionaryTypeRepository types, IAfterCommit afterCommit)
    : ICommandHandler<EnableDictionaryItemCommand, VersionedResponse<DictionaryTypeId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<DictionaryTypeId>>> Handle(EnableDictionaryItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await types.GetByCodeForUpdateAsync(command.TypeCode, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var enabled = loaded.Value.EnableItem(command.Code);
        if (enabled.IsFailure)
        {
            return enabled.Error;
        }

        DictionaryChanges.Invalidate(afterCommit, command.TypeCode);
        return new VersionedResponse<DictionaryTypeId>(loaded.Value.Id);
    }
}
