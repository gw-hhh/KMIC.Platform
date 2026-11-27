using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>调整同级全部字典项的排序</summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="ParentCode">父项编码</param>
/// <param name="OrderedCodes">字典项编码列表，按新顺序排列全部同级项（含停用项）</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record ReorderDictionaryItemsCommand(string TypeCode, long ExpectedVersion, string? ParentCode, IReadOnlyList<string> OrderedCodes) : ICommand<VersionedResponse<DictionaryTypeId>>;
