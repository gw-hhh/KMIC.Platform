using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 查询系统读取文件所需的信息
/// </summary>
/// <remarks>
/// 可读状态比下载宽
/// </remarks>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record GetReadableFileQuery(Guid FileId) : IQuery<FileDownload>;
