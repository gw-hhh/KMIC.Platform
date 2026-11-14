using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 按文件 ID 取读取内容需要的信息，经 <see cref="IFileContents"/> 给系统读取文件内容用，只允许系统身份
/// </summary>
/// <remarks>
/// 能读的状态比下载宽：已关联、可用、已解除关联都可以，因为读的是系统生成的文件或刚上传的附件，不一定已经关联到业务对象
/// </remarks>
/// <param name="FileId">文件 ID</param>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record GetReadableFileQuery(Guid FileId) : IQuery<FileDownload>;
