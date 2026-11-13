namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>
/// 删除队列里领取到的一个文件
/// </summary>
/// <param name="Id">文件 ID</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="Status">领取时的状态编码：删除中的连行一起删，已彻底删除的只删对象、行保留</param>
internal sealed record DeletionItem(Guid Id, string StorageKey, string Status);
