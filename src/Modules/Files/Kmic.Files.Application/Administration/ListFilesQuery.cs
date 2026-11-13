using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询文件列表：管理员查看文件的元数据、状态和引用，不含内容和存储键
/// </summary>
/// <param name="Q">关键字，在文件名里模糊匹配</param>
/// <param name="Status">状态编码，可以给多个</param>
/// <param name="Profile">上传档案</param>
/// <param name="UploadedBy">上传的人</param>
/// <param name="From">上传时间不早于</param>
/// <param name="To">上传时间早于</param>
/// <param name="OwnerModule">只看被这个模块的业务对象引用的文件</param>
/// <param name="OwnerType">业务对象的类型，和 OwnerModule 一起给</param>
/// <param name="OwnerId">业务对象的 ID，和 OwnerModule、OwnerType 一起给</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(FilesPermissions.FileView)]
public sealed record ListFilesQuery(
    string? Q,
    IReadOnlyList<string>? Status,
    string? Profile,
    UserId? UploadedBy,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? OwnerModule,
    string? OwnerType,
    Guid? OwnerId,
    PageRequest Page,
    SortRequest Sort) : IQuery<PagedResult<FileListItem>>;
