using Kmic.BuildingBlocks.Application.Files;

namespace Kmic.Sample.Application;

/// <summary>
/// 示例附件的上传配置
/// </summary>
/// <remarks>
/// 上传须具备“编辑示例附件”权限
/// </remarks>
[UploadProfile(
    "Sample.SpecimenAttachment",
    "示例附件",
    MaxBytes = 10 * 1024 * 1024,
    AllowedTypes = [FileType.Pdf, FileType.Jpeg, FileType.Png, FileType.Docx, FileType.Xlsx, FileType.Txt],
    Permission = SamplePermissions.Edit)]
public static class SampleUploadProfiles;
