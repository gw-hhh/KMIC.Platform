using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Kmic.Files.Application;

public sealed class FilesMetrics
{
    public const string MeterName = "Kmic.Files";

    private readonly Counter<long> _purgeSkipped;

    public FilesMetrics(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _purgeSkipped = factory.Create(MeterName).CreateCounter<long>("kmic.files.purge_skipped", "{file}", "清理时跳过、留待下次处理的到期文件数");
    }

    /// <summary>记录清理到期文件时跳过的候选文件数</summary>
    public void PurgeSkipped(string reason, int count)
    {
        if (count > 0)
        {
            _purgeSkipped.Add(count, new TagList { { "reason", reason } });
        }
    }
}
