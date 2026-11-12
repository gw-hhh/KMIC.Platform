using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 按 ID 批量取用户的联系方式，只给 Notifications 这类系统调用方通过 <see cref="IUserContacts"/> 调用
/// </summary>
/// <remarks>
/// 手机号、邮箱是个人信息，只有系统身份能取，普通用户和别的模块的用户请求都不行
/// </remarks>
/// <param name="Ids">用户 ID，一次最多 1000 个</param>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record GetUserContactsQuery(IReadOnlyCollection<UserId> Ids) : IQuery<IReadOnlyDictionary<Guid, UserContact>>;
