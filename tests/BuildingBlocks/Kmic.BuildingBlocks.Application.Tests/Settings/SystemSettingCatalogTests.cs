using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Settings;

public sealed class SystemSettingCatalogTests
{
    private static readonly Error _outOfRange = Error.BusinessRule("BuildingBlocks.ThresholdOutOfRange", "阈值超出范围");

    [Fact]
    public void Build_ValidSetting_ExposesDefaultValidationAndPermission()
    {
        var catalog = SystemSettingCatalog.Build([typeof(ApprovalThreshold)]);

        var setting = catalog.Get<ApprovalThreshold>();
        setting.Key.ShouldBe("BuildingBlocks.Approval.Threshold");
        setting.Module.ShouldBe("BuildingBlocks");
        setting.Group.ShouldBe("审批");
        setting.DefaultValue.ShouldBe(new ApprovalThreshold(50_000m));
        setting.Validate(new ApprovalThreshold(-1)).Error.ShouldBeSameAs(_outOfRange);
        setting.Validate(new ApprovalThreshold(1)).IsSuccess.ShouldBeTrue();
        Should.Throw<ArgumentException>(() => setting.Validate("不是这个类型"));
        catalog.Find("BuildingBlocks.Approval.Threshold").ShouldBeSameAs(setting);
        catalog.Find("BuildingBlocks.Removed").ShouldBeNull();
        catalog.PermissionReferences.ShouldBe([new PermissionReference("Thing.Settings.Manage", "系统设置 BuildingBlocks.Approval.Threshold")]);
    }

    [Theory]
    [InlineData(typeof(NotImplementing), "必须实现 ISystemSetting<NotImplementing>")]
    [InlineData(typeof(MutableSetting), "属性只能 init")]
    [InlineData(typeof(UnsupportedMember), "类型 Uri 不能用于设置值")]
    [InlineData(typeof(OtherModuleKey), "本模块名")]
    [InlineData(typeof(WithoutEditor), "必须声明 EditPermission")]
    [InlineData(typeof(InvalidDefault), "默认值没有通过自身的 Validate")]
    [InlineData(typeof(NotRecord), "必须是 sealed record")]
    public void Build_WrongDeclaration_RefusesWithReason(Type settingType, string reason)
    {
        var thrown = Should.Throw<DeclarationException>(() => SystemSettingCatalog.Build([settingType]));

        thrown.Problems.ShouldContain(p => p.Contains(settingType.Name, StringComparison.Ordinal) && p.Contains(reason, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_DuplicateKey_Refuses()
    {
        var thrown = Should.Throw<DeclarationException>(() => SystemSettingCatalog.Build([typeof(ApprovalThreshold), typeof(DuplicateKey)]));

        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Approval.Threshold 重复声明", StringComparison.Ordinal));
    }

    [SystemSetting("BuildingBlocks.Approval.Threshold", "审批金额阈值", Group = "审批", EditPermission = "Thing.Settings.Manage")]
    public sealed record ApprovalThreshold(decimal Value) : ISystemSetting<ApprovalThreshold>
    {
        public static ApprovalThreshold Default => new(50_000m);

        public static Result Validate(ApprovalThreshold value) => value.Value is > 0 and <= 10_000_000 ? Result.Success() : _outOfRange;
    }

    [SystemSetting("BuildingBlocks.Approval.Threshold", "重复", EditPermission = "Thing.Settings.Manage")]
    public sealed record DuplicateKey(int Value) : ISystemSetting<DuplicateKey>
    {
        public static DuplicateKey Default => new(1);

        public static Result Validate(DuplicateKey value) => Result.Success();
    }

    [SystemSetting("BuildingBlocks.NotImplementing", "没有实现接口", EditPermission = "Thing.Settings.Manage")]
    public sealed record NotImplementing(int Value);

    [SystemSetting("BuildingBlocks.Mutable", "可变", EditPermission = "Thing.Settings.Manage")]
    public sealed record MutableSetting : ISystemSetting<MutableSetting>
    {
        public static MutableSetting Default => new();

        public int Value { get; set; }

        public static Result Validate(MutableSetting value) => Result.Success();
    }

    [SystemSetting("BuildingBlocks.Unsupported", "不支持的成员", EditPermission = "Thing.Settings.Manage")]
    public sealed record UnsupportedMember(Uri Endpoint) : ISystemSetting<UnsupportedMember>
    {
        public static UnsupportedMember Default => new(new Uri("https://example.com"));

        public static Result Validate(UnsupportedMember value) => Result.Success();
    }

    [SystemSetting("Other.Key", "别的模块", EditPermission = "Thing.Settings.Manage")]
    public sealed record OtherModuleKey(int Value) : ISystemSetting<OtherModuleKey>
    {
        public static OtherModuleKey Default => new(1);

        public static Result Validate(OtherModuleKey value) => Result.Success();
    }

    [SystemSetting("BuildingBlocks.WithoutEditor", "没有修改权限")]
    public sealed record WithoutEditor(int Value) : ISystemSetting<WithoutEditor>
    {
        public static WithoutEditor Default => new(1);

        public static Result Validate(WithoutEditor value) => Result.Success();
    }

    [SystemSetting("BuildingBlocks.InvalidDefault", "默认值不合法", EditPermission = "Thing.Settings.Manage")]
    public sealed record InvalidDefault(int Value) : ISystemSetting<InvalidDefault>
    {
        public static InvalidDefault Default => new(0);

        public static Result Validate(InvalidDefault value) => value.Value > 0 ? Result.Success() : _outOfRange;
    }

    [SystemSetting("BuildingBlocks.NotRecord", "不是 record", EditPermission = "Thing.Settings.Manage")]
    public sealed class NotRecord : ISystemSetting<NotRecord>
    {
        public static NotRecord Default => new();

        public static Result Validate(NotRecord value) => Result.Success();
    }
}
