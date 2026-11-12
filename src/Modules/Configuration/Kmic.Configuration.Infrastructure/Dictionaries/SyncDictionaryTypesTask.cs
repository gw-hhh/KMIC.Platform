using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Application.Dictionaries.Sync;

namespace Kmic.Configuration.Infrastructure.Dictionaries;

/// <summary>
/// 启动任务：迁移之后、接收请求之前，把代码里声明的字典类型同步进数据库
/// </summary>
internal sealed class SyncDictionaryTypesTask(ICommandSender sender) : IStartupTask
{
    public string Name => "Configuration.SyncDictionaryTypes";

    public int Order => 0;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        var synced = await sender.SendAsync(new SyncDictionaryTypesCommand(), ct);
        return synced.IsSuccess ? Result.Success() : synced.Error;
    }
}
