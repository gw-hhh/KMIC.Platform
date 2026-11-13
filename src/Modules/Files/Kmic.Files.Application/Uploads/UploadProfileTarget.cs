namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 交给资源策略的上传档案摘要
/// </summary>
/// <param name="Name">档案名</param>
/// <param name="Permission">上传要有的权限点</param>
public sealed record UploadProfileTarget(string Name, string Permission);
