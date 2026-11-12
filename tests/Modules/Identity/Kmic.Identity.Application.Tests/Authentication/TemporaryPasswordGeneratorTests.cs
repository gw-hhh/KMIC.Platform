using Kmic.Identity.Application.Authentication;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.Identity.Application.Tests.Authentication;

public sealed class TemporaryPasswordGeneratorTests
{
    private static TemporaryPasswordGenerator Generator(PasswordOptions? options = null) => new(Options.Create(options ?? new PasswordOptions()));

    [Fact]
    public void Password_IsLongEnough_AndPassesThePasswordPolicy()
    {
        var policy = new PasswordPolicy(Options.Create(new PasswordOptions()), new EmptyWeakList());
        var generator = Generator();

        for (var i = 0; i < 50; i++)
        {
            var password = generator.Generate();

            password.Length.ShouldBeGreaterThanOrEqualTo(14);
            policy.Validate(password, "13800138000", "+8613800138000").IsSuccess.ShouldBeTrue();
        }
    }

    [Fact]
    public void Password_FollowsAStricterMinimumLength()
    {
        Generator(new PasswordOptions { MinLength = 20 }).Generate().Length.ShouldBe(20);
    }

    [Fact]
    public void Password_LeavesOutCharactersThatAreEasyToMisread()
    {
        var generator = Generator();

        var all = string.Concat(Enumerable.Range(0, 200).Select(_ => generator.Generate()));

        all.IndexOfAny(['0', 'O', '1', 'l', 'I']).ShouldBe(-1);
    }

    [Fact]
    public void EveryCall_GivesADifferentPassword()
    {
        var generator = Generator();

        Enumerable.Range(0, 100).Select(_ => generator.Generate()).Distinct(StringComparer.Ordinal).Count().ShouldBe(100);
    }

    private sealed class EmptyWeakList : IWeakPasswordList
    {
        public bool Contains(string password) => false;
    }
}
