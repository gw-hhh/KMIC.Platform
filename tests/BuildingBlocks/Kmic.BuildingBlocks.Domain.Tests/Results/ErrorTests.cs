using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Domain.Tests.Results;

public sealed class ErrorTests
{
    [Theory]
    [InlineData("NoModule")]
    [InlineData("sample.NoItems")]
    [InlineData("Sample.noItems")]
    [InlineData("Sample.No-Items")]
    [InlineData("Sample.No.Items")]
    [InlineData("")]
    public void Create_WhenCodeFormatInvalid_Throws(string code)
    {
        Should.Throw<ArgumentException>(() => Error.BusinessRule(code, "说明"));
    }

    [Fact]
    public void Factories_SetTypeAndKeepFieldErrorsOnlyForValidation()
    {
        var fieldError = new FieldError("items[2].quantity", "Range", "数量必须大于 0");

        var validation = Error.Validation("Common.ValidationFailed", "输入不合法", [fieldError]);
        var conflict = Error.Conflict("Sample.Duplicated", "重复");

        validation.Type.ShouldBe(ErrorType.Validation);
        validation.FieldErrors.ShouldBe([fieldError]);
        conflict.Type.ShouldBe(ErrorType.Conflict);
        conflict.FieldErrors.ShouldBeEmpty();
        conflict.Metadata.ShouldBeEmpty();
        conflict.DetailArgs.ShouldBeEmpty();
    }

    [Fact]
    public void WithDetailAndMetadata_ReturnNewInstance_OriginalUnchanged()
    {
        var original = Error.Conflict("Common.VersionConflict", "数据已被他人修改");

        var detailed = original.WithDetail("当前版本 43");
        var withVersion = detailed.WithMetadata(ErrorMetadataKeys.CurrentVersion, 43L);

        original.Detail.ShouldBeNull();
        original.Metadata.ShouldBeEmpty();
        detailed.Detail.ShouldBe("当前版本 43");
        withVersion.Detail.ShouldBe("当前版本 43");
        withVersion.Metadata[ErrorMetadataKeys.CurrentVersion].ShouldBe(43L);
        detailed.Metadata.ShouldBeEmpty();
    }
}
