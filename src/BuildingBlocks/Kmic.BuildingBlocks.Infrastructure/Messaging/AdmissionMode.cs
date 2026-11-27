namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal enum AdmissionMode
{
    // 软线、硬线均拒绝
    Enforce,

    // 豁免软线，仅硬线拒绝
    ExemptFromSoftLine,

    // 硬线时静默跳过写入，避免主事务回滚（适用于通知类消息）
    SkipWhenSuspended,
}
