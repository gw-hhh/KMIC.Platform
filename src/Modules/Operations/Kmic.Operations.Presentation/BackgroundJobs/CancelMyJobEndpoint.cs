using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.BackgroundJobs.Mine;

namespace Kmic.Operations.Presentation.BackgroundJobs;

internal sealed class CancelMyJobEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/my-jobs/{module}/{id}/cancel", HandleAsync)
            .Sends<CancelMyJobCommand>()
            .WithSummary("请求取消本人提交的后台作业")
            .WithDescription("排队中的作业直接取消；运行中的先标记为取消中，执行器续期检测到后发出取消信号，处理器响应后停止。已提交的批次不回滚。作业不存在或由其他用户提交时均返回 404；作业已结束或正在取消时返回 409 Operations.BackgroundJobFinished。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(
        [Description("作业所在的模块，如 Sample")] string module,
        [Description("作业 ID")] Guid id,
        ICommandSender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new CancelMyJobCommand(module, id), ct)).ToNoContent();
}
