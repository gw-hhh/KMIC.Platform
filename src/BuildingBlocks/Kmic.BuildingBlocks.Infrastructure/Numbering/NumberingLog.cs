using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

internal static partial class NumberingLog
{
    [LoggerMessage(EventId = 2501, Level = LogLevel.Warning, Message = "Number rule {Rule} reached sequence {Sequence}, the number will grow longer past {Digits} digits")]
    public static partial void SequenceNearLimit(ILogger logger, string rule, long sequence, int digits);
}
