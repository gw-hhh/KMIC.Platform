using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

internal static class ConditionalEndpoints
{
    public static readonly Type[] All = [typeof(GetCachedThingEndpoint)];

    public static ListedModule Module() => new("Conditional", All);

    internal sealed class GetCachedThingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/things/{id}", HandleAsync).Sends<GetThingQuery>().WithSummary("查询，带 ETag");

        private static async Task<IResult> HandleAsync(ThingId id, HttpContext http, IQuerySender sender, CancellationToken ct) =>
            (await sender.SendAsync(new GetThingQuery(id), ct)).ToOkWithETag(http, thing => thing.Name);
    }
}
