using Kmic.BuildingBlocks.Contracts.Snapshots;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 单据的快照来源，数据由测试直接摆放
/// </summary>
internal sealed class FakeTicketSource : ITicketSnapshotSource
{
    // 按 uuid 的字节顺序排，和数据库里的排序一致
    private static readonly Comparer<Guid> _byBytes = Comparer<Guid>.Create(
        (a, b) => a.ToByteArray(bigEndian: true).AsSpan().SequenceCompareTo(b.ToByteArray(bigEndian: true)));

    private readonly SortedDictionary<Guid, SnapshotItem<TicketSnapshot>> _items = new(_byBytes);

    /// <summary>
    /// 每次读一页之前执行，比如在扫描中途改动数据
    /// </summary>
    public Func<int, CancellationToken, Task>? BeforePage { get; set; }

    /// <summary>
    /// 读过的页数
    /// </summary>
    public int Pages { get; private set; }

    /// <summary>
    /// 摆一条快照，已有的按新版本覆盖
    /// </summary>
    public Guid Set(string title, long version, Guid? id = null)
    {
        var key = id ?? Guid.CreateVersion7();
        _items[key] = new SnapshotItem<TicketSnapshot>(new TicketSnapshot(key, title, "Open"), version);
        return key;
    }

    /// <summary>
    /// 源端物理删除
    /// </summary>
    public void Remove(Guid id) => _items.Remove(id);

    public async Task<SnapshotPage<TicketSnapshot>> GetPageAsync(Guid? after, int size, CancellationToken ct)
    {
        Pages++;
        if (BeforePage is { } before)
        {
            await before(Pages, ct);
        }

        var rest = _items.Where(i => after is null || _byBytes.Compare(i.Key, after.Value) > 0).Take(size + 1).ToList();
        var items = rest.Take(size).Select(i => i.Value).ToList();
        return new SnapshotPage<TicketSnapshot>(items, rest.Count > size ? items[^1].Snapshot.Id : null);
    }

    public Task<IReadOnlyDictionary<Guid, SnapshotItem<TicketSnapshot>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
        Task.FromResult<IReadOnlyDictionary<Guid, SnapshotItem<TicketSnapshot>>>(
            ids.Where(_items.ContainsKey).ToDictionary(id => id, id => _items[id]));
}
