using Kmic.BuildingBlocks.Application.Files;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的上传档案：标本附件
/// </summary>
/// <remarks>
/// 演示业务模块怎样声明上传档案：权限点用业务的“编辑”权限，避免任何登录用户都能往存储里塞文件
/// </remarks>
[UploadProfile(
    "Sample.SpecimenAttachment",
    "示例标本附件",
    MaxBytes = 10 * 1024 * 1024,
    AllowedTypes = [FileType.Pdf, FileType.Jpeg, FileType.Png, FileType.Docx, FileType.Xlsx, FileType.Txt],
    Permission = SamplePermissions.Edit)]
public static class SampleUploadProfiles;
