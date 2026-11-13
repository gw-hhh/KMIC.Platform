using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.SiteMessages;
using Shouldly;

namespace Kmic.Notifications.Domain.Tests.SiteMessages;

public sealed class SiteMessageTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 13, 9, 0, 0, TimeSpan.Zero);

    private static SiteMessage Created(string? link = "/sample/specimens/1") =>
        SiteMessage.Create(UserId.New(), null, "标题", "正文", link, "Sample", _now).Value;

    [Fact]
    public void Create_StartsUnread_AndKeepsWhatItWasGiven()
    {
        var recipient = UserId.New();

        var created = SiteMessage.Create(recipient, null, "  审批通过  ", "你的单据已通过", "/sample/specimens/1", "Sample", _now);

        created.IsSuccess.ShouldBeTrue();
        created.Value.RecipientUserId.ShouldBe(recipient);
        created.Value.Title.ShouldBe("审批通过", "两端的空白去掉");
        created.Value.ReadAt.ShouldBeNull();
        created.Value.SentAt.ShouldBe(_now);
    }

    [Theory]
    [InlineData("/sample/specimens/1")]
    [InlineData("/")]
    [InlineData("/a?b=c#d")]
    public void Create_AnInternalPathIsAccepted(string link) =>
        SiteMessage.Create(UserId.New(), null, "标题", "正文", link, "Sample", _now).IsSuccess.ShouldBeTrue();

    [Theory]
    [InlineData("https://evil.example.com/x")]
    [InlineData("//evil.example.com/x")]
    [InlineData("javascript:alert(1)")]
    [InlineData("sample/specimens/1")]
    [InlineData("/a\\b")]
    [InlineData("/a://b")]
    [InlineData("/a\nb")]
    public void Create_AnythingThatLeavesTheSite_IsRefused(string link)
    {
        var created = SiteMessage.Create(UserId.New(), null, "标题", "正文", link, "Sample", _now);

        created.IsFailure.ShouldBeTrue();
        created.Error.ShouldBe(NotificationsErrors.SiteMessageLinkInvalid);
    }

    [Fact]
    public void Create_WithoutALink_IsAllowed_AndTooLongTextIsCut()
    {
        var created = SiteMessage.Create(UserId.New(), null, new string('题', 500), new string('文', 5000), null, "Sample", _now).Value;

        created.Link.ShouldBeNull();
        created.Title.Length.ShouldBe(SiteMessage.TitleMaxLength);
        created.Body.Length.ShouldBe(SiteMessage.BodyMaxLength);
    }

    [Fact]
    public void MarkRead_SetsTheTime_AndASecondMarkChangesNothing()
    {
        var message = Created();

        message.MarkRead(_now.AddMinutes(1));
        message.MarkRead(_now.AddHours(5));

        message.ReadAt.ShouldBe(_now.AddMinutes(1));
    }
}
