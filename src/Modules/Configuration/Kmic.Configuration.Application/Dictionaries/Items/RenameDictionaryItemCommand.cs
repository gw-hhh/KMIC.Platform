using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Domain.Dictionaries;

namespace Kmic.Configuration.Application.Dictionaries.Items;

/// <summary>
/// 修改字典项的显示名；编码不能改，业务数据里存的是编码
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回版本冲突</param>
/// <param name="Code">字典项编码</param>
/// <param name="Name">新的显示名</param>
[RequirePermission(ConfigurationPermissions.DictionaryManage)]
[Audited]
[Idempotent]
public sealed record RenameDictionaryItemCommand(string TypeCode, long ExpectedVersion, string Code, string Name) : ICommand<VersionedResponse<DictionaryTypeId>>;
