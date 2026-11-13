using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 检查一批文件现在能不能被关联，经 <see cref="IFileAttachability"/> 给关联的互斥协议用，只允许系统身份
/// </summary>
/// <remarks>
/// 条件：状态是“可用”且是这个人上传的（系统用户可以关联没有上传人的系统生成文件），或者是“已解除关联”；
/// 不检查到期时间，已过期但还没进入删除队列的文件仍可关联。只回答能关联的，不能关联的不在结果里
/// </remarks>
/// <param name="FileIds">文件 ID</param>
/// <param name="UploaderId">关联的人</param>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record CheckAttachabilityQuery(IReadOnlyCollection<Guid> FileIds, Guid UploaderId) : IQuery<IReadOnlyList<FileSnapshot>>;
