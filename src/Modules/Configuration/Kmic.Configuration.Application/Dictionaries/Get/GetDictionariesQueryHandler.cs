using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 处理 <see cref="GetDictionariesQuery"/>
/// </summary>
public sealed class GetDictionariesQueryHandler(IDictionarySnapshots snapshots) : IQueryHandler<GetDictionariesQuery, DictionariesResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<DictionariesResponse>> Handle(GetDictionariesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var found = new List<DictionarySnapshot>();
        foreach (var code in query.Types.Distinct(StringComparer.Ordinal))
        {
            if (await snapshots.GetAsync(code, cancellationToken) is { } snapshot)
            {
                found.Add(snapshot);
            }
        }

        return new DictionariesResponse(found, ETagOf(found));
    }

    // 各类型的编码与版本号拼起来取哈希：任何一个类型变了，整体的 ETag 就变
    private static string ETagOf(IReadOnlyList<DictionarySnapshot> snapshots)
    {
        var text = string.Join(';', snapshots.Select(s => $"{s.TypeCode}={s.Version}"));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
    }
}
