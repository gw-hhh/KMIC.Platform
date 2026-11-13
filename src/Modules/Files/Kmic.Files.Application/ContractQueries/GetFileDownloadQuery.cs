using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 按文件 ID 取下载信息，经 <see cref="IFileDownloads"/> 给业务的下载查询用
/// </summary>
/// <remarks>
/// 不检查权限点、不套数据范围：知道文件 ID 就能取到下载信息（含存储键，不含内容）。调用方必须先完成业务对象的授权。
/// 返回的是下载信息而不是引用信息，所以在特性上写明例外的原因
/// </remarks>
/// <param name="FileId">文件 ID</param>
[ContractQuery(ContractAccess.Reference, ResultExemption = "返回下载信息（元数据与存储键，不含内容）：业务的下载查询要用它组装下载，调用方先完成业务对象的授权")]
public sealed record GetFileDownloadQuery(Guid FileId) : IQuery<FileDownload>;
