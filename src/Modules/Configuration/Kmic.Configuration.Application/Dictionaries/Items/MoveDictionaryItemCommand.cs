using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 层级字典里改字典项的父项，排在新父项下的最后
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回版本冲突</param>
/// <param name="Code">字典项编码</param>
/// <param name="NewParentCode">新的父项编码；不给就移到顶层。不能是自己或自己的后代</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record MoveDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code, string? NewParentCode) : ICommand<VersionedResponse<DictionaryTypeId>>;
