using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 上传动作
/// </summary>
public sealed class UploadAction : Enumeration<UploadAction>
{
    /// <summary>
    /// 上传文件
    /// </summary>
    public static readonly UploadAction Upload = new("Upload", "上传");

    private UploadAction(string code, string name)
        : base(code, name)
    {
    }
}
