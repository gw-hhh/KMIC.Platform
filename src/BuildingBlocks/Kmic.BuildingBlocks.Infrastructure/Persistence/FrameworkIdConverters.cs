using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 框架级强类型 ID 的转换器在此集中声明并由 ModuleDbContext 统一注册，各模块仅需声明自身业务的 ID
[EfCoreConverter<UserId>]
[EfCoreConverter<RoleId>]
[EfCoreConverter<SessionId>]
[EfCoreConverter<OrgUnitId>]
[EfCoreConverter<StoredFileId>]
internal sealed partial class FrameworkIdConverters;
