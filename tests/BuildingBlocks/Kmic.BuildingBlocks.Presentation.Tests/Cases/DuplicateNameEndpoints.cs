using Kmic.BuildingBlocks.Presentation.Endpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

// 两个不同位置的同名请求模型，用于验证 OpenAPI 架构名冲突检测
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
