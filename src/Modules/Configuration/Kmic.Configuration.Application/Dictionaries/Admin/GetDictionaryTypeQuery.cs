using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 查询一个字典类型的全部字典项（含停用的）与版本号，管理页使用
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
public sealed record GetDictionaryTypeQuery(string TypeCode) : IQuery<DictionaryTypeDetail>;
