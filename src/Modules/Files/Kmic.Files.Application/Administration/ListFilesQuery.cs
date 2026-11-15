using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询文件元数据列表，不含内容和存储键
/// </summary>
/// <param name="Q">文件名搜索关键字</param>
/// <param name="Status">状态编码，支持多个</param>
/// <param name="Profile">上传配置标识；生成文件为 Generated:来源</param>
/// <param name="UploadedBy">上传人或生成请求人 ID</param>
/// <param name="From">文件登记时间下限（含）</param>
/// <param name="To">文件登记时间上限（不含）</param>
/// <param name="OwnerModule">按引用文件的业务模块筛选；未指定时忽略 OwnerType 和 OwnerId</param>
/// <param name="OwnerType">业务对象类型</param>
/// <param name="OwnerId">业务对象 ID</param>
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
