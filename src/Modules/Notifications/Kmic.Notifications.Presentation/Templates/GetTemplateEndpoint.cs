using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Templates.Get;

namespace Kmic.Notifications.Presentation.Templates;

internal sealed class GetTemplateEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/templates/{key}", HandleAsync)
            .Sends<GetTemplateQuery>()
            .WithSummary("查询通知模板详情")
            .WithDescription("返回各渠道内容、代码默认内容、可用变量、示例数据及已发布版本。已移除代码声明的模板不提供默认内容、变量和示例数据。发布或恢复默认内容时，须将响应中的 version 作为预期版本号提交。");

    private static async Task<IResult> HandleAsync([Description("模板键，如 Sample.SpecimenSubmitted")] string key, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetTemplateQuery(key), ct)).ToOk();
}
