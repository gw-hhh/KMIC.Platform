using Kmic.Sample.Domain.Specimens;
using Vogen;

namespace Kmic.Sample.Infrastructure.Persistence;

[EfCoreConverter<SpecimenId>]
[EfCoreConverter<SpecimenItemId>]
[EfCoreConverter<SpecimenAttachmentId>]
internal sealed partial class SampleIdConverters;
