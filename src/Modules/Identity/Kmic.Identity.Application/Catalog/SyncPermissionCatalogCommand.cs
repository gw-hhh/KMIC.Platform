using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 同步代码声明的权限点与数据范围对象
/// </summary>
/// <remarks>
/// 启动迁移后、接收请求前执行；目录哈希未变时跳过。
/// 按声明新增、更新或改名，改名时同步替换角色中的旧权限编码。已移除声明标为退役，保留角色配置，恢复声明后重新生效。
/// 按变化更新全权限角色的目录指纹和授权版本。
/// </remarks>
[SystemOnly]
[NotAudited("目录变更通过 PermissionCatalogSynced 安全事件记录")]
public sealed record SyncPermissionCatalogCommand : ICommand<CatalogSyncResult>;
