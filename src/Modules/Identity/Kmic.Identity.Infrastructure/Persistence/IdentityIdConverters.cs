using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Vogen;

namespace Kmic.Identity.Infrastructure.Persistence;

[EfCoreConverter<GrantId>]
[EfCoreConverter<GrantAnchorId>]
[EfCoreConverter<RolePermissionEntryId>]
[EfCoreConverter<RoleDataScopeId>]
[EfCoreConverter<RefreshTokenId>]
[EfCoreConverter<OrgTreeId>]
internal sealed partial class IdentityIdConverters;
