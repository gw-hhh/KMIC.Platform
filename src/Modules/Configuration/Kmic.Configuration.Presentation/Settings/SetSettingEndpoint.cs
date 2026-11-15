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
            .WithDescription("需要查看设置权限及该设置声明的修改权限，可通过列表中的可用操作判断。值按声明类型解析并校验。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SetSettingRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetSettingCommand(key, request.Value, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
