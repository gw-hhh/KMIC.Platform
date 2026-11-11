using Kmic.BuildingBlocks.Presentation.Endpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 两个请求模型类名相同、所在位置不同：OpenAPI 架构名会冲突
/// </summary>
internal static class DuplicateNameEndpoints
{
    public static readonly Type[] All = [typeof(FirstEndpoint), typeof(SecondEndpoint)];

    internal static class First
    {
        internal sealed record SubmitRequest(string Name);
    }

    internal static class Second
    {
        internal sealed record SubmitRequest(int Count);
    }

    internal sealed class FirstEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/first", (First.SubmitRequest request) => request.Name).Sends<MeQuery>().WithSummary("第一个");
    }

    internal sealed class SecondEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/second", (Second.SubmitRequest request) => request.Count).Sends<MeQuery>().WithSummary("第二个");
    }
}
