using System.Text.Json;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 请求模型里同时有不可空和可空的 JsonElement：它们是同一个“任意 JSON 值”，不能占用同一个架构名两次
/// </summary>
internal static class AnyJsonEndpoints
{
    public static readonly Type[] All = [typeof(AnyJsonEndpoint)];

    /// <summary>
    /// 带任意 JSON 值的请求
    /// </summary>
    /// <param name="Value">要保存的值</param>
    /// <param name="Previous">之前的值，没有时为空</param>
    internal sealed record SaveValueRequest(JsonElement Value, JsonElement? Previous);

    internal sealed class AnyJsonEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/values", (SaveValueRequest request) => request.Value).Sends<MeQuery>().WithSummary("保存值");
    }
}
