using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Purge;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class PurgeObsoleteSettingEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/settings/{key}/purge", HandleAsync)
            .Sends<PurgeObsoleteSettingCommand>()
            .WithSummary("清除代码里已经不存在的设置留下的值")
            .WithDescription("仍然在代码里声明的设置不能这样清，要用恢复默认。需要敏感权限点“清除不再使用的设置值”。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SettingReasonRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PurgeObsoleteSettingCommand(key, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
