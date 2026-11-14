using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 查询可关联的文件，仅供 <see cref="IFileAttachability"/> 以系统身份调用
/// </summary>
/// <remarks>
/// 返回关联人上传的可用文件或已解除关联的文件；系统用户还可关联无上传人的可用文件。
/// 不检查到期时间，不符合关联条件的文件不出现在结果中
/// </remarks>
/// <param name="FileIds">文件 ID</param>
/// <param name="UploaderId">关联的人</param>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record CheckAttachabilityQuery(IReadOnlyCollection<Guid> FileIds, Guid UploaderId) : IQuery<IReadOnlyList<FileSnapshot>>;
