using System.Security.Cryptography;
using Kmic.BuildingBlocks.Application.Querying;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Querying;

public sealed class HmacCursorProtectorTests
{
    private static readonly string _keyA = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private static readonly string _keyB = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static HmacCursorProtector With(string? key) => new(Options.Create(new CursorOptions { SigningKey = key }));

    [Fact]
    public void ACursor_RoundTripsTheKeyAndTheId()
    {
        var protector = With(_keyA);
        var id = Guid.CreateVersion7();

        var cursor = protector.Protect("2026-11-13T08:00:00.0000000+00:00", id);

        protector.TryUnprotect(cursor, out var key, out var readId).ShouldBeTrue();
        key.ShouldBe("2026-11-13T08:00:00.0000000+00:00");
        readId.ShouldBe(id);
    }

    [Fact]
    public void ACursor_IsOpaqueAndUrlSafe()
    {
        var cursor = With(_keyA).Protect("12345", Guid.CreateVersion7());

        cursor.ShouldNotContain("12345");
        cursor.ShouldNotContain("+");
        cursor.ShouldNotContain("/");
        cursor.ShouldNotContain("=");
    }

    [Fact]
    public void ACursorSignedWithAnotherKey_IsRefused()
    {
        var cursor = With(_keyA).Protect("1", Guid.CreateVersion7());

        With(_keyB).TryUnprotect(cursor, out _, out _).ShouldBeFalse();
    }

    [Fact]
    public void ATamperedPayloadOrSignature_IsRefused()
    {
        var protector = With(_keyA);
        var cursor = protector.Protect("1", Guid.CreateVersion7());
        var parts = cursor.Split('.');
        var otherPayload = protector.Protect("999", Guid.CreateVersion7()).Split('.')[0];

        protector.TryUnprotect(otherPayload + "." + parts[1], out _, out _).ShouldBeFalse();
        protector.TryUnprotect(parts[0] + "." + parts[1][..^2] + "AA", out _, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-separator")]
    [InlineData("a.b.c")]
    [InlineData("!!!.???")]
    public void NonsenseCursors_AreRefusedWithoutThrowing(string cursor) =>
        With(_keyA).TryUnprotect(cursor, out _, out _).ShouldBeFalse();

    [Fact]
    public void TooLongACursor_IsRefused() =>
        With(_keyA).TryUnprotect(new string('a', 600), out _, out _).ShouldBeFalse();

    [Fact]
    public void WithoutAConfiguredKey_EachProcessGetsItsOwn_SoTwoInstancesDoNotAgree()
    {
        var first = With(null);
        var second = With(null);

        var cursor = first.Protect("1", Guid.CreateVersion7());

        first.TryUnprotect(cursor, out _, out _).ShouldBeTrue();
        second.TryUnprotect(cursor, out _, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("not base64!")]
    [InlineData("AAAA")]
    public void AnInvalidConfiguredKey_IsRefusedAtConstruction(string key) =>
        Should.Throw<InvalidOperationException>(() => With(key));
}
