using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Vogen;

namespace Kmic.Identity.Infrastructure.Persistence;

/// <summary>
/// 本模块全部强类型 ID 的 EF 转换器，由 Vogen 生成注册方法
/// </summary>
[EfCoreConverter<GrantId>]
[EfCoreConverter<GrantAnchorId>]
[EfCoreConverter<RolePermissionEntryId>]
[EfCoreConverter<RoleDataScopeId>]
[EfCoreConverter<RefreshTokenId>]
[EfCoreConverter<OrgTreeId>]
internal sealed partial class IdentityIdConverters;
