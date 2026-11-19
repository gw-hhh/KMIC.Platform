using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 登记本实例声明的订阅路由
/// </summary>
internal sealed class RegisterMessageRoutesTask(SubscriptionCatalog subscriptions, DataSources dataSources) : IStartupTask
{
    public string Name => "Framework.RegisterMessageRoutes";

    public int Order => -300;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        await new MessageRoutes(dataSources.Transactions(CallOrigin.Startup)).RegisterAsync(
            MessageRoutes.Declared(subscriptions), MessageRoutes.Retirements(subscriptions), BuildInfo.Version, ct);
        return Result.Success();
    }
}
