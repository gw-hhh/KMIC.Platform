using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 把代码里声明的权限点与范围对象同步进数据库
/// </summary>
/// <remarks>
/// 启动任务，迁移之后、接收请求之前执行。目录哈希未变时跳过同步。有变化时：按声明改名（角色上的旧编码原地换成新编码），
/// 新增与更新，代码里已经没有的标为退役（角色上的配置保留，重新出现时原样生效），拥有全部权限点的角色同步指纹、授权版本递增
/// </remarks>
[SystemOnly]
[NotAudited("目录变更通过 PermissionCatalogSynced 安全事件记录")]
public sealed record SyncPermissionCatalogCommand : ICommand<CatalogSyncResult>;
