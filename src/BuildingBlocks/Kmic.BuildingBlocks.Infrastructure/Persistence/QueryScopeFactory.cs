using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class QueryScopeFactory(IExecutionScopeFactory scopes) : IQueryScopeFactory
{
    public async Task<TResult> RunAsync<TContext, TResult>(CallOrigin origin, Func<TContext, CancellationToken, Task<TResult>> work, CancellationToken ct)
        where TContext : ModuleQueryDbContext
    {
        ArgumentNullException.ThrowIfNull(work);

        await using var scope = scopes.CreateScope(origin, CurrentUser.System());
        return await work(scope.ServiceProvider.GetRequiredService<TContext>(), ct);
    }
}
