using Vogen;

// 05-01 第 4.1 节的统一配置
[assembly: VogenDefaults(
    underlyingType: typeof(Guid),
    conversions: Conversions.SystemTextJson | Conversions.TypeConverter,
    parsableForPrimitives: ParsableForPrimitives.HoistMethodsAndInterfaces,
    toPrimitiveCasting: CastOperator.None,
    fromPrimitiveCasting: CastOperator.None)]
