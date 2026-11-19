using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 把代码声明的权限点与范围对象同步到目录，目录哈希没变时跳过
/// </summary>
/// <remarks>
/// 已移除的声明标为退役并保留角色配置，恢复声明后重新生效；改名时替换角色里的旧编码
/// </remarks>
[SystemOnly]
[NotAudited("目录变更通过 PermissionCatalogSynced 安全事件记录")]
public sealed record SyncPermissionCatalogCommand : ICommand<CatalogSyncResult>;
