using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Files.Domain.Errors;
using Kmic.Files.Domain.Files;
using Shouldly;

namespace Kmic.Files.Domain.Tests.Files;

public sealed class StoredFileTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 13, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _temp = TimeSpan.FromHours(24);
    private static readonly TimeSpan _retention = TimeSpan.FromDays(30);
    private static readonly string _sha = new('a', 64);

    private static StoredFile Started() =>
        StoredFile.Start(StoredFileId.New(), "dev/2026/11/abc", "合同.pdf", "Sample.ContractScan", UserId.New(), _now);

    private static StoredFile Available()
    {
        var file = Started();
        file.CompleteUpload("pdf", "application/pdf", 1234, _sha, "v1", needsScan: false, _now, _temp).IsSuccess.ShouldBeTrue();
        return file;
    }

    private static StoredFile Attached(Guid? owner = null)
    {
        var file = Available();
        file.Attach("Sample", "Specimen", owner ?? Guid.CreateVersion7(), _now).IsSuccess.ShouldBeTrue();
        return file;
    }

    [Fact]
    public void Start_RegistersAnUploadingRow_AndWritesTheFirstHistoryEntry()
    {
        var uploader = UserId.New();

        var file = StoredFile.Start(StoredFileId.New(), "dev/2026/11/abc", "合同.pdf", "Sample.ContractScan", uploader, _now);

        file.Status.ShouldBe(StoredFileStatus.Uploading);
        file.UploadedBy.ShouldBe(uploader);
        file.UploadedAt.ShouldBe(_now);
        file.FileType.ShouldBeNull();
        file.Size.ShouldBeNull();
        file.References.ShouldBeEmpty();
        file.StateTransitions.ShouldHaveSingleItem();
    }

    [Fact]
    public void CompleteUpload_WithoutAScanner_MakesItAvailable_WithATemporaryExpiry()
    {
        var file = Started();

        var result = file.CompleteUpload("pdf", "application/pdf", 1234, _sha, "v1", needsScan: false, _now, _temp);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Available);
        file.FileType.ShouldBe("pdf");
        file.ContentType.ShouldBe("application/pdf");
        file.Size.ShouldBe(1234);
        file.Sha256.ShouldBe(_sha);
        file.ObjectVersionId.ShouldBe("v1");
        file.ExpiresAt.ShouldBe(_now + _temp);
    }

    [Fact]
    public void CompleteUpload_WithAScanner_WaitsForTheVerdict()
    {
        var file = Started();
        file.CompleteUpload("pdf", "application/pdf", 1234, _sha, null, needsScan: true, _now, _temp).IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Scanning);

        var later = _now.AddMinutes(5);
        file.PassScan(later, _temp).IsSuccess.ShouldBeTrue();

        file.Status.ShouldBe(StoredFileStatus.Available);
        file.ExpiresAt.ShouldBe(later + _temp);
    }

    [Fact]
    public void CompleteUpload_OnlyWorksOnceWhileUploading()
    {
        var file = Available();

        file.CompleteUpload("pdf", "application/pdf", 1, _sha, null, needsScan: false, _now, _temp).Error.ShouldBe(FilesErrors.InvalidState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reject_FromUploadingOrScanning_KeepsTheReasonAndExpiresSoon(bool scanning)
    {
        var file = Started();
        if (scanning)
        {
            file.CompleteUpload("pdf", "application/pdf", 1, _sha, null, needsScan: true, _now, _temp);
        }

        var result = file.Reject("类型不符", _now, TimeSpan.FromDays(1));

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Rejected);
        file.RejectReason.ShouldBe("类型不符");
        file.ExpiresAt.ShouldBe(_now.AddDays(1));
    }

    [Fact]
    public void Reject_AnAvailableFile_IsNotAllowed() =>
        Available().Reject("太晚了", _now, TimeSpan.FromDays(1)).Error.ShouldBe(FilesErrors.InvalidState);

    [Fact]
    public void Attach_AvailableBecomesAttached_WithAReference_AndNoExpiry()
    {
        var file = Available();
        var owner = Guid.CreateVersion7();

        var result = file.Attach("Sample", "Specimen", owner, _now);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Attached);
        file.ExpiresAt.ShouldBeNull();
        var reference = file.References.ShouldHaveSingleItem();
        reference.IsOwnedBy("Sample", "Specimen", owner).ShouldBeTrue();
        reference.AttachedAt.ShouldBe(_now);
    }

    [Fact]
    public void Attach_TheSameOwnerAgain_IsAnIdempotentSuccess_AnotherOwnerAddsAReference()
    {
        var owner = Guid.CreateVersion7();
        var file = Attached(owner);

        file.Attach("Sample", "Specimen", owner, _now.AddMinutes(1)).IsSuccess.ShouldBeTrue();
        file.References.Count.ShouldBe(1);
        file.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now.AddMinutes(2)).IsSuccess.ShouldBeTrue();
        file.Attach("Quotation", "Quote", owner, _now.AddMinutes(3)).IsSuccess.ShouldBeTrue();

        file.References.Count.ShouldBe(3);
        file.Status.ShouldBe(StoredFileStatus.Attached);
    }

    [Fact]
    public void Attach_ADetachedFile_ComesBackWithinTheRetention()
    {
        var owner = Guid.CreateVersion7();
        var file = Attached(owner);
        file.Detach("Sample", "Specimen", owner, _now, _retention).IsSuccess.ShouldBeTrue();

        file.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now.AddDays(1)).IsSuccess.ShouldBeTrue();

        file.Status.ShouldBe(StoredFileStatus.Attached);
        file.ExpiresAt.ShouldBeNull();
    }

    [Fact]
    public void Attach_DeletingRejectedUploadingAndPurgedFiles_IsRefused()
    {
        var deleting = Available();
        deleting.Expire("到期").IsSuccess.ShouldBeTrue();
        var rejected = Started();
        rejected.Reject("类型不符", _now, TimeSpan.FromDays(1));
        var purged = Attached();
        purged.Purge("误传", UserId.New(), _now).IsSuccess.ShouldBeTrue();

        deleting.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now).Error.ShouldBe(FilesErrors.NotAttachable);
        rejected.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now).Error.ShouldBe(FilesErrors.NotAttachable);
        Started().Attach("Sample", "Specimen", Guid.CreateVersion7(), _now).Error.ShouldBe(FilesErrors.NotAttachable);
        purged.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now).Error.ShouldBe(FilesErrors.Purged);
    }

    [Fact]
    public void Attach_StopsAtTheReferenceCap()
    {
        var file = Available();
        for (var i = 0; i < StoredFile.MaxReferences; i++)
        {
            file.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now).IsSuccess.ShouldBeTrue();
        }

        file.Attach("Sample", "Specimen", Guid.CreateVersion7(), _now).Error.ShouldBe(FilesErrors.TooManyReferences);
        file.References.Count.ShouldBe(StoredFile.MaxReferences);
    }

    [Fact]
    public void Detach_TheLastReference_StartsTheRetention_AnEarlierOneDoesNot()
    {
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        var file = Attached(first);
        file.Attach("Sample", "Specimen", second, _now);

        file.Detach("Sample", "Specimen", first, _now, _retention).IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Attached);
        file.ExpiresAt.ShouldBeNull();

        var later = _now.AddDays(2);
        file.Detach("Sample", "Specimen", second, later, _retention).IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Detached);
        file.ExpiresAt.ShouldBe(later + _retention);
        file.References.ShouldBeEmpty();
    }

    [Fact]
    public void Detach_AReferenceThatIsNotThere_ChangesNothing()
    {
        var file = Attached();

        file.Detach("Sample", "Specimen", Guid.CreateVersion7(), _now, _retention).IsSuccess.ShouldBeTrue();

        file.Status.ShouldBe(StoredFileStatus.Attached);
        file.References.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData("available")]
    [InlineData("detached")]
    [InlineData("rejected")]
    public void Expire_OnlyTemporaryDetachedAndRejectedFilesGoToTheDeleteQueue(string from)
    {
        var file = from switch
        {
            "available" => Available(),
            "detached" => DetachedFile(),
            _ => RejectedFile(),
        };

        file.Expire("到期").IsSuccess.ShouldBeTrue();

        file.Status.ShouldBe(StoredFileStatus.Deleting);
    }

    [Fact]
    public void Expire_AnAttachedFile_IsRefused() => Attached().Expire("到期").Error.ShouldBe(FilesErrors.InvalidState);

    [Theory]
    [InlineData("uploading")]
    [InlineData("available")]
    [InlineData("attached")]
    [InlineData("detached")]
    [InlineData("rejected")]
    public void Purge_AnyLiveState_BecomesPurged_AndKeepsTheReferences(string from)
    {
        var file = from switch
        {
            "uploading" => Started(),
            "available" => Available(),
            "attached" => Attached(),
            "detached" => DetachedFile(),
            _ => RejectedFile(),
        };
        var references = file.References.Count;
        var by = UserId.New();

        var result = file.Purge("误传了敏感文件", by, _now);

        result.IsSuccess.ShouldBeTrue();
        file.Status.ShouldBe(StoredFileStatus.Purged);
        file.PurgedAt.ShouldBe(_now);
        file.PurgedBy.ShouldBe(by);
        file.PurgeReason.ShouldBe("误传了敏感文件");
        file.References.Count.ShouldBe(references);
    }

    [Fact]
    public void Purge_ADeletingOrAlreadyPurgedFile_IsRefused()
    {
        var deleting = Available();
        deleting.Expire("到期");
        var purged = Available();
        purged.Purge("误传", UserId.New(), _now);

        deleting.Purge("误传", UserId.New(), _now).Error.ShouldBe(FilesErrors.InvalidState);
        purged.Purge("再来一次", UserId.New(), _now).Error.ShouldBe(FilesErrors.InvalidState);
    }

    [Fact]
    public void RecordHistoryPurge_OnlyAfterThePurge()
    {
        var file = Available();
        file.RecordHistoryPurge("工单 1234", _now).Error.ShouldBe(FilesErrors.InvalidState);
        file.Purge("误传", UserId.New(), _now);

        file.RecordHistoryPurge("工单 1234", _now.AddDays(1)).IsSuccess.ShouldBeTrue();

        file.HistoryPurgedAt.ShouldBe(_now.AddDays(1));
        file.HistoryPurgeNote.ShouldBe("工单 1234");
    }

    [Fact]
    public void RegisterGenerated_IsAvailableAtOnce_WithTheGivenExpiry_AndNoUploaderForSystemFiles()
    {
        var expires = _now.AddDays(7);

        var file = StoredFile.RegisterGenerated(
            StoredFileId.New(), "dev/2026/11/gen", null, "导出.xlsx", "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 5000, _sha, "Sample.ExportSpecimens", null, _now, expires);

        file.Status.ShouldBe(StoredFileStatus.Available);
        file.Profile.ShouldBe("Generated:Sample.ExportSpecimens");
        file.UploadedBy.ShouldBeNull();
        file.ExpiresAt.ShouldBe(expires);
        file.Size.ShouldBe(5000);
        file.StateTransitions.Count.ShouldBe(2);
    }

    private static StoredFile DetachedFile()
    {
        var owner = Guid.CreateVersion7();
        var file = Attached(owner);
        file.Detach("Sample", "Specimen", owner, _now, _retention);
        return file;
    }

    private static StoredFile RejectedFile()
    {
        var file = Started();
        file.Reject("类型不符", _now, TimeSpan.FromDays(1));
        return file;
    }
}
