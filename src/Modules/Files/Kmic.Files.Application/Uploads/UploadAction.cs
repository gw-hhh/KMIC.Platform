using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Application.Uploads;

public sealed class UploadAction : Enumeration<UploadAction>
{
    public static readonly UploadAction Upload = new("Upload", "上传");

    private UploadAction(string code, string name)
        : base(code, name)
    {
    }
}
