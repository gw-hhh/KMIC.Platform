using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>调整字典项的父项</summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="Code">字典项编码</param>
/// <param name="NewParentCode">新父项编码</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record MoveDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code, string? NewParentCode) : ICommand<VersionedResponse<DictionaryTypeId>>;
