using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 已废弃的端点和同样功能的未废弃端点
/// </summary>
internal static class DeprecatedEndpoints
{
    public static readonly Type[] All = [typeof(OldMeEndpoint), typeof(OldFailingEndpoint), typeof(CurrentMeEndpoint)];

    internal sealed class OldMeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/old-me", HandleAsync).Sends<MeQuery>().Deprecated(new DateOnly(2026, 11, 4), new DateOnly(2027, 5, 4)).WithSummary("旧接口");

        private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
            (await sender.SendAsync(new MeQuery(), ct)).ToOk();
    }

    internal sealed class OldFailingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/old-failing", HandleAsync).Sends<GetThingQuery>().Deprecated(new DateOnly(2026, 11, 4), new DateOnly(2027, 5, 4)).WithSummary("旧接口，会失败");

        private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
            (await sender.SendAsync(new GetThingQuery(Support.ThingId.New()), ct)).ToOk();
    }

    internal sealed class CurrentMeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/me", HandleAsync).Sends<MeQuery>().WithSummary("新接口");

        private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
            (await sender.SendAsync(new MeQuery(), ct)).ToOk();
    }
}
