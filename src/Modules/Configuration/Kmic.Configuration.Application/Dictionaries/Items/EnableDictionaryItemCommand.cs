using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 启用字典项：它的父项必须是启用的
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回版本冲突</param>
/// <param name="Code">字典项编码</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record EnableDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code) : ICommand<VersionedResponse<DictionaryTypeId>>;
