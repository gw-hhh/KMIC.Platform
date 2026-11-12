using System.Text.Json;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Shouldly;

namespace Kmic.Notifications.Contracts.Tests;

public sealed class SendNotificationTests
{
    private static readonly Guid _user = Guid.CreateVersion7();

    [Fact]
    public void For_PlainTemplate_UsesDeclaredKeyAndKeepsPropertyNames()
    {
        var request = SendNotification.For(
            new ApprovedModel("S-0001", "张三"),
            [Recipient.OfUser(_user)],
            "Test.Approved:1",
            priority: NotificationPriority.High);

        request.TemplateKey.ShouldBe("Test.Approved");
        request.DedupKey.ShouldBe("Test.Approved:1");
        request.Priority.ShouldBe(NotificationPriority.High);
        request.To.ShouldHaveSingleItem().UserId.ShouldBe(_user);
        request.Model.GetProperty("Number").GetString().ShouldBe("S-0001");
        request.Model.GetProperty("ApproverName").GetString().ShouldBe("张三");
    }

    [Fact]
    public void For_ModelRoundTripsThroughSharedOptions()
    {
        var request = SendNotification.For(new ApprovedModel("S-0002", "李四"), [Recipient.OfEmail("a@example.com", RecipientRole.Cc)], "k");

        var model = request.Model.Deserialize<ApprovedModel>(NotificationModelJson.Options);

        model.ShouldBe(new ApprovedModel("S-0002", "李四"));
        request.To[0].Role.ShouldBe(RecipientRole.Cc);
    }

    [Fact]
    public void ModelJson_UnknownProperty_IsRejected()
    {
        var json = JsonDocument.Parse("""{"Number":"1","ApproverName":"x","Extra":"y"}""").RootElement;

        Should.Throw<JsonException>(() => json.Deserialize<ApprovedModel>(NotificationModelJson.Options));
    }

    [Fact]
    public void For_SecretTemplateWithPlainMessage_Throws() =>
        Should.Throw<ArgumentException>(() => SendNotification.For(new CodeModel("123456"), [Recipient.OfPhone("+8613800000000")], "k"));

    [Fact]
    public void ForSecret_SecretTemplate_Works()
    {
        var request = SendSecretNotification.For(new CodeModel("123456"), [Recipient.OfPhone("+8613800000000")], "Test.Code:abc");

        request.TemplateKey.ShouldBe("Test.Code");
        request.Model.GetProperty("Code").GetString().ShouldBe("123456");
        request.To.ShouldHaveSingleItem().Phone.ShouldBe("+8613800000000");
    }

    [Fact]
    public void ForSecret_PlainTemplate_Throws() =>
        Should.Throw<ArgumentException>(() => SendSecretNotification.For(new ApprovedModel("1", "x"), [Recipient.OfUser(_user)], "k"));

    [Fact]
    public void For_ModelWithoutTemplateAttribute_Throws() =>
        Should.Throw<ArgumentException>(() => SendNotification.For(new UndeclaredModel("x"), [Recipient.OfUser(_user)], "k"));

    [Fact]
    public void For_BlankDedupKey_Throws() =>
        Should.Throw<ArgumentException>(() => SendNotification.For(new ApprovedModel("1", "x"), [Recipient.OfUser(_user)], " "));

    [Fact]
    public void DefaultTextBody_FallsBackToDefaultBody()
    {
        ApprovedModel.DefaultBody.ShouldBe("<p>{{ ApproverName }} 已审批 {{ Number }}</p>");
        TextBodyOf<ApprovedModel>().ShouldBe(ApprovedModel.DefaultBody);
        TextBodyOf<CodeModel>().ShouldBe("验证码 {{ Code }}");
    }

    private static string TextBodyOf<TModel>()
        where TModel : INotificationModel<TModel> => TModel.DefaultTextBody;

    [NotificationTemplate("Test.Approved", "测试审批通过", Channels = NotificationChannels.Email | NotificationChannels.InApp)]
    private sealed record ApprovedModel(string Number, string ApproverName) : INotificationModel<ApprovedModel>
    {
        public static ApprovedModel Example => new("S-2026-0001", "张三");

        public static string DefaultSubject => "示例 {{ Number }} 已审批通过";

        public static string DefaultBody => "<p>{{ ApproverName }} 已审批 {{ Number }}</p>";
    }

    [NotificationTemplate("Test.Code", "测试验证码", Channels = NotificationChannels.Sms, CarriesSecret = true)]
    private sealed record CodeModel(string Code) : INotificationModel<CodeModel>, ISmsTemplateModel
    {
        public static CodeModel Example => new("123456");

        public static string DefaultSubject => "验证码";

        public static string DefaultBody => "<b>验证码</b> {{ Code }}";

        public static string DefaultTextBody => "验证码 {{ Code }}";

        public IReadOnlyDictionary<string, string> ToSmsParameters() => new Dictionary<string, string>(StringComparer.Ordinal) { ["code"] = Code };
    }

    private sealed record UndeclaredModel(string Text) : INotificationModel<UndeclaredModel>
    {
        public static UndeclaredModel Example => new("x");

        public static string DefaultSubject => "s";

        public static string DefaultBody => "b";
    }
}
