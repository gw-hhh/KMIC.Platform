using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 对上传档案的动作
/// </summary>
public sealed class UploadAction : Enumeration<UploadAction>
{
    /// <summary>
    /// 往这个档案上传文件
    /// </summary>
    public static readonly UploadAction Upload = new("Upload", "上传");

    private UploadAction(string code, string name)
        : base(code, name)
    {
    }
}
