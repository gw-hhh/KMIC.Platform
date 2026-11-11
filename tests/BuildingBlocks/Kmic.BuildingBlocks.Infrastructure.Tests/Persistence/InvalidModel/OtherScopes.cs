using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence.InvalidModel;

[DataScopeObjectGroup("Other")]
public static class OtherScopes
{
    [DataScopeObject("其他模块的数据")]
    public const string Thing = "Other.Thing";
}
