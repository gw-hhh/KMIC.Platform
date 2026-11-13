using Kmic.Sample.Domain.Specimens;
using Vogen;

namespace Kmic.Sample.Infrastructure.Persistence;

/// <summary>
/// 本模块全部强类型 ID 的 EF 转换器，由 Vogen 生成注册方法
/// </summary>
[EfCoreConverter<SpecimenId>]
[EfCoreConverter<SpecimenItemId>]
[EfCoreConverter<SpecimenAttachmentId>]
internal sealed partial class SampleIdConverters;
