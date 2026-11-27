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
            .WithSummary("清除已移除声明的设置值")
            .WithDescription("仅可清除代码中已移除声明的设置（仍在使用的请用恢复默认）；需敏感权限“清除不再使用的设置值”");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SettingReasonRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PurgeObsoleteSettingCommand(key, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
