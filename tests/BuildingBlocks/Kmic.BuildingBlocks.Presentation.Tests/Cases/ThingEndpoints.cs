using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 一组正常的端点，覆盖 Result 转 HTTP 的每种写法和请求解析的几种情况。路径是 /api/v1/{模块}/...
/// </summary>
internal static class ThingEndpoints
{
    public static readonly Type[] All =
    [
        typeof(GetThingEndpoint), typeof(CreateThingEndpoint), typeof(DeleteThingEndpoint), typeof(ExportThingsEndpoint),
        typeof(RenameThingEndpoint), typeof(EchoEndpoint), typeof(BoomEndpoint), typeof(PayloadTooLargeEndpoint), typeof(MeEndpoint),
    ];

    public static ListedModule Module(string name = "Samples") => new(name, All);

    internal sealed record CreateThingRequest(string Name);

    internal sealed record RenameThingRequest(long ExpectedVersion, string Name);

    internal sealed class GetThingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/things/{id}", HandleAsync).Sends<GetThingQuery>().WithSummary("查询");

        private static async Task<IResult> HandleAsync(ThingId id, IQuerySender sender, CancellationToken ct) =>
            (await sender.SendAsync(new GetThingQuery(id), ct)).ToOk();
    }

    internal sealed class CreateThingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/things", HandleAsync).Sends<CreateThingCommand>().WithSummary("创建");

        private static async Task<IResult> HandleAsync(CreateThingRequest request, ICommandSender sender, CancellationToken ct) =>
            (await sender.SendAsync(new CreateThingCommand(request.Name), ct)).ToCreated(id => $"/api/v1/samples/things/{id.Value}");
    }

    internal sealed class DeleteThingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapDelete("/things/{id}", HandleAsync).Sends<DeleteThingCommand>().WithSummary("删除");

        private static async Task<IResult> HandleAsync(ThingId id, ICommandSender sender, CancellationToken ct) =>
            (await sender.SendAsync(new DeleteThingCommand(id), ct)).ToNoContent();
    }

    internal sealed class ExportThingsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/things/export", HandleAsync).Sends<StartExportCommand>().WithSummary("导出");

        private static async Task<IResult> HandleAsync(ICommandSender sender, CancellationToken ct) =>
            (await sender.SendAsync(new StartExportCommand(), ct)).ToAccepted(jobId => $"/api/v1/operations/jobs/{jobId}");
    }

    internal sealed class RenameThingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/things/{id}/rename", HandleAsync).Sends<RenameThingCommand>().WithSummary("改名");

        private static async Task<IResult> HandleAsync(ThingId id, RenameThingRequest request, ICommandSender sender, CancellationToken ct) =>
            (await sender.SendAsync(new RenameThingCommand(id, request.ExpectedVersion, request.Name), ct)).ToOk();
    }

    internal sealed class EchoEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/echo", HandleAsync).Sends<EchoCommand>().WithSummary("回显");

        private static async Task<IResult> HandleAsync(EchoPayload request, ICommandSender sender, CancellationToken ct) =>
            (await sender.SendAsync(new EchoCommand(request), ct)).ToOk();
    }

    internal sealed class BoomEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/boom", Handle).Sends<MeQuery>().WithSummary("抛异常");

        private static string Handle() => throw new InvalidOperationException("内部细节：连接串 Host=secret");
    }

    internal sealed class PayloadTooLargeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/too-large", Handle).Sends<MeQuery>().WithSummary("请求体太大");

        private static string Handle() => throw new BadHttpRequestException("body too large", StatusCodes.Status413PayloadTooLarge);
    }

    internal sealed class MeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/me", HandleAsync).Sends<MeQuery>().WithSummary("当前用户");

        private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
            (await sender.SendAsync(new MeQuery(), ct)).ToOk();
    }
}
