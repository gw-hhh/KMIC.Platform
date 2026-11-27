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
            .WithDescription("全量替换自定义设置（null 表示沿用代码默认）；保存后尝试同步调度器，同步失败不影响保存（以列表 inSync 为准）");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, SetJobOverrideRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetJobOverrideCommand(name, request.Enabled, request.Cron, request.ExpectedVersion), ct)).ToOk();
}
