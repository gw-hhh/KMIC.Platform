using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Catalog;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 启动任务：迁移之后、接收请求之前，把代码里声明的权限点与范围对象同步进数据库
/// </summary>
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
