using Microsoft.Extensions.DependencyInjection;

namespace Spike01.BuildingBlocks;

/// <summary>
/// 原型的框架服务注册
/// </summary>
public static class BuildingBlocksServiceCollectionExtensions
{
    public static IServiceCollection AddSpikeBuildingBlocks(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<ICommandSender>(sp => sp.GetRequiredService<ISender>());
        services.AddScoped<IQuerySender>(sp => sp.GetRequiredService<ISender>());
        services.AddScoped<IContractQuerySender, ContractQuerySender>();
        services.AddScoped<ExecutionContext>();
        services.AddScoped<IExecutionContext>(sp => sp.GetRequiredService<ExecutionContext>());
        services.AddScoped<PipelineTrace>();
        return services;
    }
}
