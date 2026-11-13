using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Notifications;

public sealed class NotificationTemplateCatalogTests
{
    [Fact]
    public void Build_ReadsTheDeclarationAndTheDefaultsOfTheModel()
    {
        var catalog = NotificationTemplateCatalog.Build([typeof(TemplateSamples.Approved), typeof(TemplateSamples.Code)]);

        var approved = catalog.Find("BuildingBlocks.Approved").ShouldNotBeNull();
        approved.DisplayName.ShouldBe("已审批");
        approved.Module.ShouldBe("BuildingBlocks");
        approved.Channels.ShouldBe(NotificationChannels.Email | NotificationChannels.InApp);
        approved.SenderProfile.ShouldBe(NotificationTemplateAttribute.DefaultSenderProfile);
        approved.CarriesSecret.ShouldBeFalse();
        approved.DefaultSubject.ShouldBe("单据 {{ Number }} 已通过");
        approved.DefaultBody.ShouldBe("<p>{{ Approver }} 通过了 {{ Number }}</p>");
        approved.DefaultTextBody.ShouldBe("{{ Approver }} 通过了 {{ Number }}");
        approved.Example.ShouldBe(TemplateSamples.Approved.Example);
        approved.Variables.ShouldBe(["Approver", "Number"]);
    }

    [Fact]
    public void Build_ATextBodyThatIsNotGiven_FallsBackToTheMainBody_AndASecretTemplateIsMarked()
    {
        var catalog = NotificationTemplateCatalog.Build([typeof(TemplateSamples.Code)]);

        var code = catalog.Find("BuildingBlocks.Code").ShouldNotBeNull();
        code.DefaultTextBody.ShouldBe(code.DefaultBody);
        code.CarriesSecret.ShouldBeTrue();
        catalog.Find("BuildingBlocks.Nobody").ShouldBeNull();
    }

    [Theory]
    [InlineData(typeof(TemplateSamples.WrongPrefix), "要以声明它的模块名")]
    [InlineData(typeof(TemplateSamples.NoChannel), "至少要有一个渠道")]
    [InlineData(typeof(TemplateSamples.SecretMail), "不能发邮件")]
    [InlineData(typeof(TemplateSamples.SmsWithoutMapping), "ISmsTemplateModel")]
    [InlineData(typeof(TemplateSamples.NotAModel), "INotificationModel")]
    public void Build_AWrongDeclaration_RefusesWithTheReason(Type modelType, string reason)
    {
        var thrown = Should.Throw<DeclarationException>(() => NotificationTemplateCatalog.Build([modelType]));

        thrown.Problems.ShouldContain(p => p.Contains(reason, StringComparison.Ordinal) && p.Contains(modelType.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_TheSameKeyTwice_IsRefused()
    {
        var thrown = Should.Throw<DeclarationException>(() => NotificationTemplateCatalog.Build([typeof(TemplateSamples.Approved), typeof(TemplateSamples.DuplicateKey)]));

        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Approved", StringComparison.Ordinal) && p.Contains("重复", StringComparison.Ordinal));
    }

    [Fact]
    public void FromAssemblies_FindsEveryDeclaredModelInTheAssembly_AndRefusesTheBadOnes()
    {
        // 这个测试程序集里同时有对的和故意写错的声明：整体读取必须失败，不会悄悄只收对的
        var thrown = Should.Throw<DeclarationException>(() => NotificationTemplateCatalog.FromAssemblies([typeof(TemplateSamples).Assembly]));

        thrown.Problems.Count.ShouldBeGreaterThanOrEqualTo(5);
    }
}
