using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Templates.Sync;

namespace Kmic.Notifications.Infrastructure.Templates;

/// <summary>
/// 启动任务：迁移之后、接收请求之前，把代码里声明的通知模板同步进数据库
/// </summary>
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
