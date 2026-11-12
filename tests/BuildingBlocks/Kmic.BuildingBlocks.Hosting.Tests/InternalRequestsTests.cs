using System.Net;
using Kmic.BuildingBlocks.Hosting.Health;
using Shouldly;

namespace Kmic.BuildingBlocks.Hosting.Tests;

/// <summary>
/// 依赖状态端点只给内网直连的请求看
/// </summary>
public sealed class InternalRequestsTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.0.0.1")]
    [InlineData("10.255.255.254")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.1")]
    [InlineData("192.168.1.20")]
    [InlineData("169.254.10.1")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("fd12:3456:789a::1")]
    [InlineData("fc00::5")]
    [InlineData("::ffff:10.1.2.3")]
    [InlineData("::ffff:127.0.0.1")]
    public void PrivateAddress_IsInternal(string address) => InternalRequests.IsInternal(From(address)).ShouldBeTrue();

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("203.0.113.9")]
    [InlineData("172.15.255.1")]
    [InlineData("172.32.0.1")]
    [InlineData("192.169.0.1")]
    [InlineData("11.0.0.1")]
    [InlineData("2001:db8::1")]
    [InlineData("fe00::1")]
    [InlineData("::ffff:8.8.8.8")]
    public void PublicAddress_IsNotInternal(string address) => InternalRequests.IsInternal(From(address)).ShouldBeFalse();

    [Fact]
    public void NoRemoteAddress_IsInternal() => InternalRequests.IsInternal(new DefaultHttpContext()).ShouldBeTrue();

    [Theory]
    [InlineData("X-Forwarded-For")]
    [InlineData("Forwarded")]
    [InlineData("X-Real-IP")]
    public void ForwardingHeader_MakesEvenAPrivateAddressExternal(string header)
    {
        var context = From("10.0.0.1");
        context.Request.Headers[header] = "203.0.113.9";

        InternalRequests.IsInternal(context).ShouldBeFalse();
    }

    private static DefaultHttpContext From(string address)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        return context;
    }
}
