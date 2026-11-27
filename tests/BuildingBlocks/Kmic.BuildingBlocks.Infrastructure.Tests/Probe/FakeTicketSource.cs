using Kmic.BuildingBlocks.Contracts.Snapshots;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class FakeTicketSource : ITicketSnapshotSource
{
    // 按 UUID 大端字节序排序，与数据库中的排序规则保持一致
    private static readonly Comparer<Guid> _byBytes = Comparer<Guid>.Create(
        (a, b) => a.ToByteArray(bigEndian: true).AsSpan().SequenceCompareTo(b.ToByteArray(bigEndian: true)));

    private readonly SortedDictionary<Guid, SnapshotItem<TicketSnapshot>> _items = new(_byBytes);

    // 每次读页前执行，用于在扫描中途模拟数据变更
    public Func<int, CancellationToken, Task>? BeforePage { get; set; }

    public int Pages { get; private set; }

    public Guid Set(string title, long version, Guid? id = null)
    {
        var key = id ?? Guid.CreateVersion7();
        _items[key] = new SnapshotItem<TicketSnapshot>(new TicketSnapshot(key, title, "Open"), version);
        return key;
    }

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
