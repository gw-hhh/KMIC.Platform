namespace Kmic.BuildingBlocks.Infrastructure.Files;

internal sealed class UploadTooLargeException(long maxBytes) : Exception($"上传内容超过上限 {maxBytes} 字节。");
