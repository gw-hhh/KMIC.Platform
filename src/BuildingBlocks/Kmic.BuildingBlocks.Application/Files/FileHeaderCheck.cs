namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 看文件头是不是这种类型：只读开头的几 KB，不能把整个文件读进来
/// </summary>
/// <param name="header">文件开头的字节，最多 <see cref="FileTypes.HeaderLength" /> 个</param>
public delegate bool FileHeaderCheck(ReadOnlySpan<byte> header);
