using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.Sync;

/// <summary>
/// 同步代码声明的通知模板目录
/// </summary>
/// <remarks>
/// 启动迁移后、接收请求前执行：登记新增模板，更新有变化的声明，将已移除声明的模板标为退役。
/// 同步锁已被其他实例持有时跳过，不等待
/// </remarks>
[SystemOnly]
[NotAudited("按代码声明同步模板目录")]
public sealed record SyncTemplateCatalogCommand : ICommand<TemplateSyncResult>;
