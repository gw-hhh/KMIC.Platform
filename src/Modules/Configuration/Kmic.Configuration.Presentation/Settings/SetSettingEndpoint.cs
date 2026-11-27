using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Set;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class SetSettingEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/settings/{key}", HandleAsync)
            .Sends<SetSettingCommand>()
            .WithSummary("修改系统设置")
            .WithDescription("需同时具备查看权限与该设置的修改权限，值按声明类型解析校验");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SetSettingRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetSettingCommand(key, request.Value, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
