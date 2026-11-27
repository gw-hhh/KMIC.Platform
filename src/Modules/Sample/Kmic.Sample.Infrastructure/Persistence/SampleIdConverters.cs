using Kmic.Sample.Domain.SpecimenCategories;
using Kmic.Sample.Domain.Specimens;
using Vogen;

namespace Kmic.Sample.Infrastructure.Persistence;

[EfCoreConverter<SpecimenId>]
[EfCoreConverter<SpecimenItemId>]
[EfCoreConverter<SpecimenAttachmentId>]
[EfCoreConverter<SpecimenCategoryId>]
internal sealed partial class SampleIdConverters;
