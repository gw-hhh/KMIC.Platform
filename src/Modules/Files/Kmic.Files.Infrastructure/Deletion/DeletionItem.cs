namespace Kmic.Files.Infrastructure.Deletion;

// 删除队列中已领取的文件（Status 为领取时的状态编码）
internal sealed record DeletionItem(Guid Id, string StorageKey, string Status);
