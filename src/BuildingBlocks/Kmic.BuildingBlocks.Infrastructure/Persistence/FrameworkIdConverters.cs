using Kmic.BuildingBlocks.Domain.Identifiers;
using Vogen;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

// 框架身份 ID 的 EF 转换器在这里声明一次，由 ModuleDbContext 统一注册；模块只声明自己的 ID（原型 #4）
[EfCoreConverter<UserId>]
[EfCoreConverter<RoleId>]
[EfCoreConverter<SessionId>]
[EfCoreConverter<OrgUnitId>]
internal sealed partial class FrameworkIdConverters;
