using Microsoft.Extensions.Logging;

namespace Kmic.Files.Infrastructure;

/// <summary>
/// Files 模块的日志
/// </summary>
internal static partial class FilesLog
{
    [LoggerMessage(EventId = 13001, Level = LogLevel.Warning, Message = "Deleting the object of file {FileId} failed, it stays in the deletion queue and is retried later")]
    public static partial void ObjectDeletionFailed(ILogger logger, Exception exception, Guid fileId);
}
