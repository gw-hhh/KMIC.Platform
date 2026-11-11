using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.Limits;

/// <summary>
/// 请求超时：超过时间的请求被取消，返回 504 和统一格式的错误
/// </summary>
public sealed class RequestTimeoutTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SlowRequest_IsCancelledAndAnswered504()
    {
        var configuration = new Dictionary<string, string?>(StringComparer.Ordinal) { ["Kmic:Http:RequestTimeout"] = "00:00:01" };
        await using var api = await ApiHost.StartAsync([new ListedModule("Samples", LimitEndpoints.All)], services: s => s.AddSingleton(new RequestGate()), configuration: configuration);

        var response = await api.Client.GetAsync("/api/v1/samples/slow", Ct);

        var body = await response.ShouldBeProblemAsync(504, "Common.RequestTimeout");

        // 不可重试：无法判断操作是否已经生效
        body.GetProperty("retryable").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task FastRequest_IsNotAffected()
    {
        var configuration = new Dictionary<string, string?>(StringComparer.Ordinal) { ["Kmic:Http:RequestTimeout"] = "00:00:01" };
        await using var api = await ApiHost.StartAsync([new ListedModule("Samples", LimitEndpoints.All)], services: s => s.AddSingleton(new RequestGate()), configuration: configuration);

        var response = await api.Client.GetAsync("/api/v1/samples/light", Ct);

        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}
