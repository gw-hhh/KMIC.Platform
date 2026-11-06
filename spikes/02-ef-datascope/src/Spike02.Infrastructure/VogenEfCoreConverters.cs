using Spike02.Domain;
using Vogen;

namespace Spike02.Infrastructure;

// 原型 #4：在 Infrastructure 中为 Domain 的强类型 ID 生成 EF 值转换器（05-01 第 4.2 节）
[EfCoreConverter<SampleId>]
[EfCoreConverter<UserId>]
[EfCoreConverter<OrgUnitId>]
internal sealed partial class VogenEfCoreConverters;
