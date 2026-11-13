using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.Sync;

/// <summary>
/// 把代码里声明的通知模板同步进数据库
/// </summary>
/// <remarks>
/// 启动任务，迁移之后、接收请求之前执行。新声明的登记，声明变了的更新，代码里已经删掉的标为退役。
/// 多个实例同时启动时只有一个做同步，其余直接跳过
/// </remarks>
[SystemOnly]
[NotAudited("目录同步，没有操作人")]
public sealed record SyncTemplateCatalogCommand : ICommand<TemplateSyncResult>;
