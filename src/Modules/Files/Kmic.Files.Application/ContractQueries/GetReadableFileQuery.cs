using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>查询系统读取文件所需的信息（允许读取的状态范围比下载更宽）</summary>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record GetReadableFileQuery(Guid FileId) : IQuery<FileDownload>;
