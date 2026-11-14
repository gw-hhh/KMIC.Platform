using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Reset;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class ResetSettingEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/settings/{key}/reset", HandleAsync)
            .Sends<ResetSettingCommand>()
            .WithSummary("恢复系统设置默认值")
            .WithDescription("移除自定义值，并记录新值为 null 的变更历史。权限要求与修改设置相同。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SettingReasonRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetSettingCommand(key, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
