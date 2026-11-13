namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 上传的内容超过了上传档案的上限：读流时发现，中断写入
/// </summary>
internal sealed class UploadTooLargeException(long maxBytes) : Exception($"上传的内容超过了上限 {maxBytes} 字节。");
