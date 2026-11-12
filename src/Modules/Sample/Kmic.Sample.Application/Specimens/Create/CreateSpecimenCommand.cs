using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>
/// 创建示例标本
/// </summary>
/// <remarks>
/// 提交的操作可能因为超时被重试，所以要带幂等键。负责人和所属组织取自当前用户，不由请求给出
/// </remarks>
/// <param name="Title">标题</param>
/// <param name="Items">明细，至少一项</param>
[RequirePermission(SamplePermissions.Create)]
[Audited]
[Idempotent]
public sealed record CreateSpecimenCommand(string Title, IReadOnlyList<CreateSpecimenItemInput> Items) : ICommand<CreatedResponse<SpecimenId>>;
