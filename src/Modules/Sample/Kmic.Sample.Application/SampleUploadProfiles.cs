using Kmic.BuildingBlocks.Application.Files;

namespace Kmic.Sample.Application;

[UploadProfile(
    "Sample.SpecimenAttachment",
    "示例附件",
    MaxBytes = 10 * 1024 * 1024,
    AllowedTypes = [FileType.Pdf, FileType.Jpeg, FileType.Png, FileType.Docx, FileType.Xlsx, FileType.Txt],
    Permission = SamplePermissions.Edit)]
public static class SampleUploadProfiles;
