using Kmic.BuildingBlocks.Application.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Execution;

internal sealed class ExecutionScopeFactory(IServiceScopeFactory scopes) : IExecutionScopeFactory
{
    public AsyncServiceScope CreateScope(CallOrigin origin, ICurrentUser user, IAccessContext? access = null)
    {
        var scope = scopes.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ExecutionScope>().Initialize(origin, user, access);
        return scope;
    }
}
