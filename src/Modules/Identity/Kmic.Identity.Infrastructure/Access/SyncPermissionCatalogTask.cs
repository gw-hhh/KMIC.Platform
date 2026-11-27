using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Catalog;

namespace Kmic.Identity.Infrastructure.Access;

internal sealed class SyncPermissionCatalogTask(ICommandSender sender) : IStartupTask
{
    public string Name => "Identity.SyncPermissionCatalog";

    public int Order => 0;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        var synced = await sender.SendAsync(new SyncPermissionCatalogCommand(), ct);
        return synced.IsSuccess ? Result.Success() : synced.Error;
    }
}
