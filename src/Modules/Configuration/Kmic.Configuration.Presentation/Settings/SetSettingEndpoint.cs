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
            .WithSummary("修改一个系统设置")
            .WithDescription("需要“查看设置”权限点，另外还要这个设置自己声明的修改权限点（列表里每项都带着当前用户能不能改）。值按设置的值类型读出后执行它自己的校验，通不过返回逐字段的错误。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("设置键")] string key, SetSettingRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetSettingCommand(key, request.Value, request.Reason, request.ExpectedVersion), ct)).ToOk();
}
