using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>将代码声明的权限点和范围对象同步至数据库</summary>
[SystemOnly]
[NotAudited("目录变更通过 PermissionCatalogSynced 安全事件记录")]
public sealed record SyncPermissionCatalogCommand : ICommand<CatalogSyncResult>;
