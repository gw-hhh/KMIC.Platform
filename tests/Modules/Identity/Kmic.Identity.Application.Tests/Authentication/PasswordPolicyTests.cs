using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Tests.Authentication;

public sealed class PasswordPolicyTests
{
    private readonly PasswordPolicy _policy = new(Options.Create(new PasswordOptions()), new FixedWeakList("qwertyuiop123"));

    [Fact]
    public void LongEnoughPassphrase_IsAccepted() =>
        _policy.Validate("correct horse battery staple", "13800138000", "+8613800138000").ShouldBeSuccess();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short1!")]
    [InlineData("123456789")]
    public void TooShort_IsRefused(string? password) =>
        _policy.Validate(password, "zhang", null).ShouldBeFailure(IdentityErrors.PasswordTooWeak);

    [Fact]
    public void TooLong_IsRefused() =>
        _policy.Validate(new string('a', 129), "zhang", null).ShouldBeFailure(IdentityErrors.PasswordTooWeak);

    [Fact]
    public void OnTheWeakList_IsRefused_IgnoringCase() =>
        _policy.Validate("QWERTYUIOP123", "zhang", null).ShouldBeFailure(IdentityErrors.PasswordTooWeak);

    [Fact]
    public void SameAsTheAccount_IsRefused() =>
        _policy.Validate("zhangsan-account", "ZhangSan-Account", null).ShouldBeFailure(IdentityErrors.PasswordTooWeak);

    [Theory]
    [InlineData("+8613800138000")]
    [InlineData("13800138000")]
    public void SameAsTheMobile_IsRefused_InBothForms(string password) =>
        _policy.Validate(password.PadRight(10, '0'), "other-account", "+8613800138000").ShouldBeFailure(IdentityErrors.PasswordTooWeak);

    [Fact]
    public void CustomLengths_AreHonored()
    {
        var strict = new PasswordPolicy(Options.Create(new PasswordOptions { MinLength = 16 }), new FixedWeakList());

        strict.Validate("0123456789abcde", "zhang", null).ShouldBeFailure(IdentityErrors.PasswordTooWeak);
        strict.Validate("0123456789abcdef", "zhang", null).ShouldBeSuccess();
    }

    private sealed class FixedWeakList(params string[] passwords) : IWeakPasswordList
    {
        public bool Contains(string password) => passwords.Contains(password, StringComparer.OrdinalIgnoreCase);
    }
}
