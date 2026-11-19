using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.Sync;

/// <summary>
/// 同步代码声明的通知模板目录
/// </summary>
[SystemOnly]
[NotAudited("按代码声明同步模板目录")]
public sealed record SyncTemplateCatalogCommand : ICommand<TemplateSyncResult>;
