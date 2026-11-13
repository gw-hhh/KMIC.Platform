using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class TemplateRepository(NotificationsDbContext context)
    : EfRepository<NotificationsDbContext, NotificationTemplate, NotificationTemplateId>(context), ITemplateRepository
{
    protected override Error NotFound => NotificationsErrors.TemplateNotFound;

    // 发布和恢复要看全部版本才能算下一个版本号
    protected override IQueryable<NotificationTemplate> Aggregate(IQueryable<NotificationTemplate> set) => set.Include(t => t.Versions);

    public async Task<Result<NotificationTemplate>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct)
    {
        var ids = await Context.Set<NotificationTemplate>().Where(t => t.Key == key).Select(t => t.Id).Take(1).ToListAsync(ct);
        return ids.Count == 1 ? await GetForUpdateAsync(ids[0], expectedVersion, ct) : NotificationsErrors.TemplateNotFound;
    }

    public async Task<IReadOnlyList<NotificationTemplate>> ListAllAsync(CancellationToken ct) =>
        await Context.Set<NotificationTemplate>().ToListAsync(ct);

    public async Task<IReadOnlyDictionary<TemplateChannel, TemplateVersion>> GetActiveVersionsAsync(string key, CancellationToken ct)
    {
        var template = await Context.Set<NotificationTemplate>().AsNoTracking().Where(t => t.Key == key).Select(t => new { t.Id, t.ActiveVersions }).SingleOrDefaultAsync(ct);
        if (template is null)
        {
            return new Dictionary<TemplateChannel, TemplateVersion>();
        }

        var active = NotificationTemplate.ParseActiveVersions(template.ActiveVersions);
        if (active.Count == 0)
        {
            return new Dictionary<TemplateChannel, TemplateVersion>();
        }

        var numbers = active.Values.ToList();
        var versions = await Context.Set<TemplateVersion>().AsNoTracking().Where(v => v.TemplateId == template.Id && numbers.Contains(v.Version)).ToListAsync(ct);
        return versions
            .Where(v => active.TryGetValue(v.Channel.Code, out var number) && number == v.Version)
            .ToDictionary(v => v.Channel);
    }
}
