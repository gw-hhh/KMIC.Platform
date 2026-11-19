using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ScheduledJobs.Set;

namespace Kmic.Operations.Presentation.ScheduledJobs;

internal sealed class SetJobOverrideEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/scheduled-jobs/{name}/override", HandleAsync)
            .Sends<SetJobOverrideCommand>()
            .WithSummary("修改定时任务的启用状态和执行时间")
            .WithDescription("整体替换自定义设置，null 表示使用代码默认值。保存后尝试同步至集群调度器，同步失败不影响保存。列表中的 inSync 表示实际调度状态是否与配置一致。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, SetJobOverrideRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetJobOverrideCommand(name, request.Enabled, request.Cron, request.ExpectedVersion), ct)).ToOk();
}
