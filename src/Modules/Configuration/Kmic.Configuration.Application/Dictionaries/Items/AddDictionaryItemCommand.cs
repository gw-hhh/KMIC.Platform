using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>添加字典项</summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="Code">编码</param>
/// <param name="Name">显示名</param>
/// <param name="ParentCode">父项编码</param>
/// <param name="SortOrder">排序值</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record AddDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code, string Name, string? ParentCode, int? SortOrder) : ICommand<VersionedResponse<DictionaryTypeId>>;
