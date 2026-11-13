using Kmic.BuildingBlocks.Application.Files;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Files;

public sealed class UploadProfileCatalogTests
{
    [UploadProfile("BuildingBlocks.ContractScan", "合同扫描件", MaxBytes = 20 * 1024 * 1024, AllowedTypes = [FileType.Pdf, FileType.Jpeg, FileType.Png], Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.Photo", "照片", MaxBytes = 5 * 1024 * 1024, AllowedTypes = [FileType.Jpeg], Permission = "BuildingBlocks.Contract.Edit")]
    private static class Good;

    [UploadProfile("Sales.ContractScan", "别的模块的名字", MaxBytes = 1024, AllowedTypes = [FileType.Pdf], Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.NoSize", "没写大小", AllowedTypes = [FileType.Pdf], Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.TooBig", "大得离谱", MaxBytes = UploadProfileAttribute.AbsoluteMaxBytes + 1, AllowedTypes = [FileType.Pdf], Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.NoTypes", "没写类型", MaxBytes = 1024, Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.UnknownType", "不认识的类型", MaxBytes = 1024, AllowedTypes = ["exe"], Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.DuplicateType", "类型重复", MaxBytes = 1024, AllowedTypes = [FileType.Pdf, FileType.Pdf], Permission = "BuildingBlocks.Contract.Edit")]
    [UploadProfile("BuildingBlocks.NoPermission", "没写权限点", MaxBytes = 1024, AllowedTypes = [FileType.Pdf])]
    [UploadProfile("BuildingBlocks.", "只有前缀", MaxBytes = 1024, AllowedTypes = [FileType.Pdf], Permission = "BuildingBlocks.Contract.Edit")]
    private static class Bad;

    [UploadProfile("BuildingBlocks.ContractScan", "重名", MaxBytes = 1024, AllowedTypes = [FileType.Pdf], Permission = "BuildingBlocks.Contract.Edit")]
    private static class Duplicate;

    [Fact]
    public void Build_ReadsEveryProfile_SortedByName_WithItsModule()
    {
        var catalog = UploadProfileCatalog.Build([typeof(Good)]);

        catalog.All.Select(p => p.Name).ShouldBe(["BuildingBlocks.ContractScan", "BuildingBlocks.Photo"]);
        var scan = catalog.Find("BuildingBlocks.ContractScan").ShouldNotBeNull();
        scan.Module.ShouldBe("BuildingBlocks");
        scan.DisplayName.ShouldBe("合同扫描件");
        scan.MaxBytes.ShouldBe(20 * 1024 * 1024);
        scan.AllowedTypes.ShouldBe([FileType.Pdf, FileType.Jpeg, FileType.Png]);
        scan.Permission.ShouldBe("BuildingBlocks.Contract.Edit");
        scan.Allows(FileType.Png).ShouldBeTrue();
        scan.Allows(FileType.Gif).ShouldBeFalse();
        catalog.Find("BuildingBlocks.Nope").ShouldBeNull();
        catalog.LargestMaxBytes.ShouldBe(20 * 1024 * 1024);
        catalog.PermissionReferences.Select(r => r.Permission).Distinct(StringComparer.Ordinal).Single().ShouldBe("BuildingBlocks.Contract.Edit");
    }

    [Fact]
    public void Build_WrongDeclarations_ListsEveryProblem()
    {
        var thrown = Should.Throw<DeclarationException>(() => UploadProfileCatalog.Build([typeof(Bad)]));

        thrown.Problems.ShouldContain(p => p.Contains("Sales.ContractScan", StringComparison.Ordinal) && p.Contains("BuildingBlocks.", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.NoSize", StringComparison.Ordinal) && p.Contains("MaxBytes", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.TooBig", StringComparison.Ordinal) && p.Contains("MaxBytes", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.NoTypes", StringComparison.Ordinal) && p.Contains("AllowedTypes", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.UnknownType", StringComparison.Ordinal) && p.Contains("exe", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.DuplicateType", StringComparison.Ordinal) && p.Contains("重复", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.NoPermission", StringComparison.Ordinal) && p.Contains("Permission", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains("“BuildingBlocks.”", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_TheSameNameTwice_IsRefused()
    {
        var thrown = Should.Throw<DeclarationException>(() => UploadProfileCatalog.Build([typeof(Good), typeof(Duplicate)]));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("BuildingBlocks.ContractScan");
    }

    [Fact]
    public void Empty_HasNoProfiles()
    {
        UploadProfileCatalog.Empty.All.ShouldBeEmpty();
        UploadProfileCatalog.Empty.LargestMaxBytes.ShouldBe(0);
        UploadProfileCatalog.Empty.Find("anything").ShouldBeNull();
    }
}
