using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 查询可关联的文件，仅供 <see cref="IFileAttachability"/> 以系统身份调用
/// </summary>
/// <param name="FileIds">文件 ID</param>
/// <param name="UploaderId">关联人 ID</param>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record CheckAttachabilityQuery(IReadOnlyCollection<Guid> FileIds, Guid UploaderId) : IQuery<IReadOnlyList<FileSnapshot>>;
