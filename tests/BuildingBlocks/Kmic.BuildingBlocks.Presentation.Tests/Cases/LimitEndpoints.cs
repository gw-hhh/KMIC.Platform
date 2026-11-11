using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Limits;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 声明了并发限制的端点，和一个处理时间很长的端点
/// </summary>
internal static class LimitEndpoints
{
    public static readonly Type[] All = [typeof(HeavyEndpoint), typeof(HeavyPerUserEndpoint), typeof(HeavyBothEndpoint), typeof(SlowEndpoint), typeof(LightEndpoint)];

    internal sealed class HeavyEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/heavy", HandleAsync).Sends<MeQuery>().RequireConcurrency(ConcurrencyPolicies.Heavy).WithSummary("每实例限制");

        private static async Task<string> HandleAsync(RequestGate gate, CancellationToken ct)
        {
            await gate.PassAsync(ct);
            return "done";
        }
    }

    internal sealed class HeavyPerUserEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/heavy-per-user", HandleAsync).Sends<MeQuery>().RequireConcurrency(ConcurrencyPolicies.HeavyPerUser).WithSummary("每用户限制");

        private static async Task<string> HandleAsync(RequestGate gate, CancellationToken ct)
        {
            await gate.PassAsync(ct);
            return "done";
        }
    }

    internal sealed class HeavyBothEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/heavy-both", HandleAsync).Sends<MeQuery>().RequireConcurrency(ConcurrencyPolicies.Heavy, ConcurrencyPolicies.HeavyPerUser).WithSummary("两个限制");

        private static async Task<string> HandleAsync(RequestGate gate, CancellationToken ct)
        {
            await gate.PassAsync(ct);
            return "done";
        }
    }

    internal sealed class SlowEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/slow", HandleAsync).Sends<MeQuery>().WithSummary("永远不会结束");

        // 只在请求被取消（超时）时结束
        private static async Task<string> HandleAsync(CancellationToken ct)
        {
            await new TaskCompletionSource().Task.WaitAsync(ct);
            return "never";
        }
    }

    internal sealed class LightEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/light", () => "ok").Sends<MeQuery>().WithSummary("没有并发限制");
    }
}
