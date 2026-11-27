using System.Net;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

// 按预设顺序返回状态码，耗尽后持续返回最后一个状态码
internal sealed class FakePartnerServer(params HttpStatusCode[] responses) : HttpMessageHandler
{
    private int _calls;

    public int Calls => Volatile.Read(ref _calls);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var index = Interlocked.Increment(ref _calls) - 1;
        var status = responses[Math.Min(index, responses.Length - 1)];
        return Task.FromResult(new HttpResponseMessage(status) { RequestMessage = request });
    }
}
