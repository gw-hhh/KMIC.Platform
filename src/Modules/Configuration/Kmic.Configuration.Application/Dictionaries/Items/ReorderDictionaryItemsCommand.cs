using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 调整同一父项下全部字典项的次序
/// </summary>
/// <remarks>
/// 必须给全：缺项或多项都拒绝，避免别人同时添加的项被悄悄丢掉
/// </remarks>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回版本冲突</param>
/// <param name="ParentCode">父项编码；顶层不给</param>
/// <param name="OrderedCodes">按新次序排好的全部字典项编码</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record ReorderDictionaryItemsCommand(string TypeCode, long ExpectedVersion, string? ParentCode, IReadOnlyList<string> OrderedCodes) : ICommand<VersionedResponse<DictionaryTypeId>>;
