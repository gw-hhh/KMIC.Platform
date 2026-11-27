using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>查询全部字典类型</summary>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
public sealed record ListDictionaryTypesQuery : IQuery<IReadOnlyList<DictionaryTypeListItem>>;
