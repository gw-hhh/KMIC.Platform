namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 一种文件类型：编码、显示名、内容类型、扩展名，和判断文件头的方法
/// </summary>
/// <param name="Code">编码，见 <see cref="FileType" /></param>
/// <param name="DisplayName">显示名</param>
/// <param name="ContentType">内容类型：下载时用它，不用上传时客户端声明的值</param>
/// <param name="Extensions">扩展名，小写、带点；第一个是规范写法</param>
/// <param name="MatchesHeader">文件头是否符合这种类型</param>
public sealed record FileTypeDefinition(string Code, string DisplayName, string ContentType, IReadOnlyList<string> Extensions, FileHeaderCheck MatchesHeader);
