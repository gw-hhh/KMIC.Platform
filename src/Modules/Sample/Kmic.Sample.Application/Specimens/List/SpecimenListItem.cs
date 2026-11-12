using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.List;

/// <summary>
/// 列表里的一行示例标本
/// </summary>
/// <param name="Id">标本</param>
/// <param name="Number">编号</param>
/// <param name="Title">标题</param>
/// <param name="Status">状态</param>
/// <param name="CreatedAt">创建时间</param>
public sealed record SpecimenListItem(SpecimenId Id, string Number, string Title, SpecimenStatus Status, DateTimeOffset CreatedAt);
