using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 查询系统读取文件所需的信息
/// </summary>
/// <remarks>
/// 可读状态比下载宽：已关联、可用、已解除关联的文件都能读，系统生成或刚上传的文件不一定已关联到业务对象
/// </remarks>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record GetReadableFileQuery(Guid FileId) : IQuery<FileDownload>;
