using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence.Repositories;

internal sealed class AuditRepository(OperationsDbContext context)
    : EfRepository<OperationsDbContext, AuditLog, AuditLogId>(context), IAuditRepository
{
    protected override Error NotFound => OperationsErrors.AuditNotFound;

    public async Task<bool> ExistsAsync(AuditLogId id, CancellationToken ct) =>
        await Context.Set<AuditLog>().AnyAsync(a => a.Id == id, ct);
}
