using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;
using Vogen;

namespace Kmic.Operations.Infrastructure.Persistence;

[EfCoreConverter<AuditLogId>]
[EfCoreConverter<ScheduledJobSettingId>]
internal sealed partial class OperationsIdConverters;
