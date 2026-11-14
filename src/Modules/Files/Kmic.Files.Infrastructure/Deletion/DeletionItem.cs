namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>
/// 删除队列中已领取的文件
/// </summary>
/// <param name="Id">文件 ID</param>
/// <param name="StorageKey">对象在存储里的键</param>
/// <param name="Status">领取时的状态编码：删除中则删除对象及数据库记录，已彻底删除则保留记录</param>
internal sealed record DeletionItem(Guid Id, string StorageKey, string Status);
