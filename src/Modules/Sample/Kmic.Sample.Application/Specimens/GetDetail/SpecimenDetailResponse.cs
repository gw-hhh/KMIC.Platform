using Kmic.Sample.Application.Specimens.Shared;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.GetDetail;

/// <summary>
/// 示例标本的详情
/// </summary>
/// <param name="Id">标本</param>
/// <param name="Version">并发版本号，修改时作为 expectedVersion 带回</param>
/// <param name="Number">编号</param>
/// <param name="Title">标题</param>
/// <param name="Status">状态</param>
/// <param name="Items">明细</param>
/// <param name="Attachments">附件</param>
/// <param name="AllowedActions">当前用户现在可以执行的动作，如 Submit、Close</param>
public sealed record SpecimenDetailResponse(
    SpecimenId Id,
    long Version,
    string Number,
    string Title,
    SpecimenStatus Status,
    IReadOnlyList<SpecimenItemResponse> Items,
    IReadOnlyList<SpecimenAttachmentResponse> Attachments,
    IReadOnlyList<string> AllowedActions);
