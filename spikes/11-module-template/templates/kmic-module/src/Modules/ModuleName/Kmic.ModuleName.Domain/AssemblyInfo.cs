using Vogen;

// 本模块强类型 ID 的统一配置：Guid、只经 From / Value 转换
[assembly: VogenDefaults(
    underlyingType: typeof(Guid),
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
