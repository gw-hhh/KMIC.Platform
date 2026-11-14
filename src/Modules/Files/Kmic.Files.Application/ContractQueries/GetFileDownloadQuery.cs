using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 按文件 ID 获取下载信息，供业务下载查询通过 <see cref="IFileDownloads"/> 调用
/// </summary>
/// <remarks>
/// 不检查权限点或数据范围，调用方必须先完成业务对象授权。结果含元数据和存储键，不含文件内容
/// </remarks>
[ContractQuery(ContractAccess.Reference, ResultExemption = "返回供业务下载使用的元数据和存储键，不含内容；调用方须先完成业务对象授权")]
public sealed record GetFileDownloadQuery(Guid FileId) : IQuery<FileDownload>;
