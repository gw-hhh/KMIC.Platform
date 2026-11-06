using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Spike06.Tests;

// 保存前把带 MirrorTo 的属性的当前值复制到影子属性：新增时总是复制，修改时只在属性被修改时复制
public sealed class MirrorInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Mirror(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Mirror(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Mirror(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.FindAnnotation(MirrorExtensions.Annotation)?.Value is string shadow
                    && (entry.State == EntityState.Added || property.IsModified))
                {
                    entry.Property(shadow).CurrentValue = property.CurrentValue;
                }
            }
        }
    }
}
