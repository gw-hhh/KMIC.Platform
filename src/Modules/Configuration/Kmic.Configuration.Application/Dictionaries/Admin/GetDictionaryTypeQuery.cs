using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 查询字典类型详情，包含已停用字典项
/// </summary>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
public sealed record GetDictionaryTypeQuery(string TypeCode) : IQuery<DictionaryTypeDetail>;
