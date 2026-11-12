using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

/// <summary>
/// 取一个字典类型的快照，只给其他模块通过 <see cref="IDictionaries"/> 调用
/// </summary>
/// <param name="TypeCode">字典类型编码</param>
[ContractQuery(ContractAccess.Reference, ResultExemption = "字典快照含类型信息与全部字典项，不是单个引用的名称与状态，但同样只读、对所有登录用户一致")]
public sealed record GetDictionarySnapshotQuery(string TypeCode) : IQuery<DictionarySnapshot?>;
