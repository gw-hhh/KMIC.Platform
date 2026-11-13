using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 框架 ID 的转换器在这里声明一次，由 ModuleDbContext 统一注册，模块只声明自己的 ID
[EfCoreConverter<UserId>]
[EfCoreConverter<RoleId>]
[EfCoreConverter<SessionId>]
[EfCoreConverter<OrgUnitId>]
[EfCoreConverter<StoredFileId>]
internal sealed partial class FrameworkIdConverters;
