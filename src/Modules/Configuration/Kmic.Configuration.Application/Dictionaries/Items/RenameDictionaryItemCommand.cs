using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 修改字典项的显示名；编码不能改，业务数据里存的是编码
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="Code">字典项编码</param>
/// <param name="Name">新的显示名</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record RenameDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code, string Name) : ICommand<VersionedResponse<DictionaryTypeId>>;
