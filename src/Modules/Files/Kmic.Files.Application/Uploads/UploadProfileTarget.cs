namespace Kmic.Files.Application.Uploads;

/// <summary>上传权限检查所需的配置信息</summary>
/// <param name="Name">上传配置标识</param>
/// <param name="Permission">上传所需权限点</param>
public sealed record UploadProfileTarget(string Name, string Permission);
