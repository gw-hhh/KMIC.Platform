using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Presentation.Administration;

/// <summary>
/// 查询文件列表的参数
/// </summary>
/// <param name="Q">关键字，在文件名里模糊匹配</param>
/// <param name="Status">状态编码，重复参数名可以给多个，如 status=Available&amp;status=Attached</param>
/// <param name="Profile">上传档案</param>
/// <param name="UploadedBy">上传的人</param>
/// <param name="From">上传时间不早于</param>
/// <param name="To">上传时间早于</param>
/// <param name="OwnerModule">只看被这个模块的业务对象引用的文件</param>
/// <param name="OwnerType">业务对象的类型，和 ownerModule 一起给</param>
/// <param name="OwnerId">业务对象的 ID，和 ownerModule、ownerType 一起给</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -uploadedAt,name：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段</param>
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
