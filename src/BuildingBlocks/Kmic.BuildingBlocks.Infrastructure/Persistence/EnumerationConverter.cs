using Kmic.BuildingBlocks.Domain.Enumerations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 智能枚举存为编码；库里的编码找不到对应取值说明数据已损坏，读取时抛异常
/// </summary>
internal sealed class EnumerationConverter<TEnumeration>()
    : ValueConverter<TEnumeration, string>(value => value.Code, code => Enumeration<TEnumeration>.FromCode(code))
    where TEnumeration : Enumeration<TEnumeration>;
