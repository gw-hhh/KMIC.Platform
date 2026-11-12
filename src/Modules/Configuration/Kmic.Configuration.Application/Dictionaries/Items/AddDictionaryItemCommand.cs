using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 添加字典项
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回版本冲突</param>
/// <param name="Code">编码，字母、数字、点、短横线、下划线，1 到 50 位，创建后不能改</param>
/// <param name="Name">显示名</param>
/// <param name="ParentCode">父项编码；只有层级字典可以给</param>
/// <param name="SortOrder">次序；不给就排在同级最后</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record AddDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code, string Name, string? ParentCode, int? SortOrder) : ICommand<VersionedResponse<DictionaryTypeId>>;
