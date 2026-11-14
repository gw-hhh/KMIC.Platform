using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Sync;

/// <summary>
/// 将代码中的字典类型声明同步到数据库
/// </summary>
/// <remarks>
/// 新类型自动登记，已有类型更新显示名、层级属性和状态，已移除声明的类型标为退役。
/// 在启动迁移后、接收请求前执行，通过咨询锁避免多实例同时同步；不修改管理员维护的字典项
/// </remarks>
[SystemOnly]
[NotAudited("目录同步，结果记入启动日志")]
public sealed record SyncDictionaryTypesCommand : ICommand<DictionarySyncResult>;
