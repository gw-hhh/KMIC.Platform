using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Sync;

/// <summary>
/// 把代码里声明的字典类型同步进数据库：登记新类型、更新显示名与层级属性、把代码里已经删除的类型标为退役
/// </summary>
/// <remarks>
/// 启动时迁移之后、接收请求之前执行，多个实例同时启动也安全：先拿咨询锁，后到的实例看到的已经是同步好的结果。
/// 类型的字典项是管理员维护的数据，同步从不动它们
/// </remarks>
[SystemOnly]
[NotAudited("目录同步，结果记入启动日志")]
public sealed record SyncDictionaryTypesCommand : ICommand<DictionarySyncResult>;
