using Microsoft.CodeAnalysis;

namespace Kmic.BuildingBlocks.Analyzers.Common;

internal static class KnownTypes
{
    public const string Error = "Kmic.BuildingBlocks.Domain.Results.Error";
    public const string Result = "Kmic.BuildingBlocks.Domain.Results.Result";
    public const string ResultOfT = "Kmic.BuildingBlocks.Domain.Results.Result`1";
    public const string CommonErrors = "Kmic.BuildingBlocks.Domain.Errors.CommonErrors";
    public const string Entity = "Kmic.BuildingBlocks.Domain.Entities.Entity`1";
    public const string DomainInvariantException = "Kmic.BuildingBlocks.Domain.Entities.DomainInvariantException";
    public const string DomainEvent = "Kmic.BuildingBlocks.Domain.Events.IDomainEvent";
    public const string Enumeration = "Kmic.BuildingBlocks.Domain.Enumerations.Enumeration`1";
    public const string StateMachine = "Kmic.BuildingBlocks.Domain.StateMachines.StateMachine";

    public static bool InheritsFrom(ITypeSymbol? type, INamedTypeSymbol? genericDefinition)
    {
        if (genericDefinition is null)
        {
            return false;
        }

        for (var current = type?.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, genericDefinition))
            {
                return true;
            }
        }

        return false;
    }

    public static bool Implements(ITypeSymbol type, INamedTypeSymbol? @interface) =>
        @interface is not null && type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, @interface));
}
