using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 批量读取字典并生成 ETag
/// </summary>
public sealed class GetDictionariesQueryHandler(IDictionarySnapshots snapshots) : IQueryHandler<GetDictionariesQuery, DictionariesResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<DictionariesResponse>> Handle(GetDictionariesQuery query, CancellationToken cancellationToken)
    {
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

    // ETag 包含各类型的编码和版本号，任一类型更新后都会变化
    private static string ETagOf(IReadOnlyList<DictionarySnapshot> snapshots)
    {
        var text = string.Join(';', snapshots.Select(s => $"{s.TypeCode}={s.Version}"));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
    }
}
