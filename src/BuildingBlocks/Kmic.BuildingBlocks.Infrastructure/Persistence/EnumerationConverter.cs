using Kmic.BuildingBlocks.Domain.Enumerations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class EnumerationConverter<TEnumeration>()
    : ValueConverter<TEnumeration, string>(value => value.Code, code => Enumeration<TEnumeration>.FromCode(code))
    where TEnumeration : Enumeration<TEnumeration>;
