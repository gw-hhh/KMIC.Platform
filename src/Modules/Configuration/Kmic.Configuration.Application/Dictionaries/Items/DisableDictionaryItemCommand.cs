using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 停用字典项：历史数据仍然显示“已停用”，新数据不能再选；下面有启用的子项时拒绝
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回版本冲突</param>
/// <param name="Code">字典项编码</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record DisableDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code) : ICommand<VersionedResponse<DictionaryTypeId>>;
