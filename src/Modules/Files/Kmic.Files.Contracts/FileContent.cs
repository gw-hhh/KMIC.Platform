namespace Kmic.Files.Contracts;

/// <summary>
/// 打开的文件内容
/// </summary>
/// <param name="Name">文件名</param>
/// <param name="ContentType">内容类型</param>
/// <param name="Size">字节数</param>
/// <param name="Content">内容的流，由调用方释放</param>
public sealed record FileContent(string Name, string ContentType, long Size, Stream Content);
