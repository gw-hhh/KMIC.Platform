using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 调整同一父项下全部字典项的次序
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">字典类型的预期版本号，不匹配时返回版本冲突</param>
/// <param name="ParentCode">父项编码；未指定时调整顶层字典项</param>
/// <param name="OrderedCodes">按新顺序排列的全部同级字典项编码（含停用项），不得重复</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record ReorderDictionaryItemsCommand(string TypeCode, long ExpectedVersion, string? ParentCode, IReadOnlyList<string> OrderedCodes) : ICommand<VersionedResponse<DictionaryTypeId>>;
