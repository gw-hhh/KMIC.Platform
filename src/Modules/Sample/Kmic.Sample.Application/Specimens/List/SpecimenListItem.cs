using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.List;

/// <summary>
/// 示例列表项
/// </summary>
/// <param name="Id">示例 ID</param>
/// <param name="Number">编号</param>
/// <param name="Title">标题</param>
/// <param name="Status">状态</param>
/// <param name="CreatedAt">创建时间</param>
public sealed record SpecimenListItem(SpecimenId Id, string Number, string Title, SpecimenStatus Status, DateTimeOffset CreatedAt);
