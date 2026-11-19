using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 查询文件列表的参数
/// </summary>
/// <param name="Q">文件名搜索关键字</param>
/// <param name="Status">状态编码</param>
/// <param name="Profile">上传配置标识</param>
/// <param name="UploadedBy">上传人或生成请求人 ID</param>
/// <param name="From">文件登记时间下限（含）</param>
/// <param name="To">文件登记时间上限（不含）</param>
/// <param name="OwnerModule">按引用文件的业务模块筛选</param>
/// <param name="OwnerType">业务对象类型</param>
/// <param name="OwnerId">业务对象 ID</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListFilesRequest(
    string? Q,
    string[]? Status,
    string? Profile,
    UserId? UploadedBy,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? OwnerModule,
    string? OwnerType,
    Guid? OwnerId,
    int Page = 1,
    int PageSize = 20,
    string? Sort = null);
