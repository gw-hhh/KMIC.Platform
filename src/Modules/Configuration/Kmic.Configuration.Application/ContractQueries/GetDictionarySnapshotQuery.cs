using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

/// <summary>
/// 供其他模块通过 <see cref="IDictionaries"/> 读取字典快照
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
[ContractQuery(ContractAccess.Reference, ResultExemption = "只读字典快照包含类型信息和全部字典项，不限于引用的名称和状态；所有登录用户读取相同内容")]
public sealed record GetDictionarySnapshotQuery(string TypeCode) : IQuery<DictionarySnapshot?>;
