using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Get;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class ListSettingsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/settings", HandleAsync)
            .Sends<ListSettingsQuery>()
            .WithSummary("查询全部系统设置")
            .WithDescription("代码里声明的设置：值里的成员清单、默认值、当前值、是否被修改过、修改它需要的权限点，以及当前用户能不能改；另列代码里已经不存在、值行还留着的设置。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListSettingsQuery(), ct)).ToOk();
}
