namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>基于文件头部字节（不超过 <see cref="FileTypes.HeaderLength" /> 字节）校验文件类型。</summary>
public delegate bool FileHeaderCheck(ReadOnlySpan<byte> header);
