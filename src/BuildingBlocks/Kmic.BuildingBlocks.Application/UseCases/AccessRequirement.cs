namespace Kmic.BuildingBlocks.Application.UseCases;

public enum AccessRequirement
{
    Permission,
    Authenticated,
    Anonymous,
    SystemOnly,
    ContractQuery,
}
