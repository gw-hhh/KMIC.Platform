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
            .WithSummary("把系统设置恢复成默认值")
            .WithDescription("删除被修改过的值，并记一条新值为空的变更。需要的权限点同修改。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SettingReasonRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetSettingCommand(key, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
