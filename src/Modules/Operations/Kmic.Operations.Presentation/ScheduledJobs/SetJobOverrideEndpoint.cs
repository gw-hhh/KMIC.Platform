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
            .WithSummary("改一个定时任务的开关和执行时间")
            .WithDescription("整体替换覆盖设置：给空的项恢复成跟着代码里的默认值。保存后同步到调度器，整个集群即时生效；同步失败不影响保存，列表里 inSync 会是 false。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, SetJobOverrideRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetJobOverrideCommand(name, request.Enabled, request.Cron, request.ExpectedVersion), ct)).ToOk();
}
