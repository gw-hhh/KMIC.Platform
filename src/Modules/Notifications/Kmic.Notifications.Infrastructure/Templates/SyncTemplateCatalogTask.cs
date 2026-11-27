using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Templates.Sync;

namespace Kmic.Notifications.Infrastructure.Templates;

internal sealed class SyncTemplateCatalogTask(ICommandSender sender) : IStartupTask
{
    public string Name => "Notifications.SyncTemplateCatalog";

    public int Order => 0;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        var synced = await sender.SendAsync(new SyncTemplateCatalogCommand(), ct);
        return synced.IsSuccess ? Result.Success() : synced.Error;
    }
}
