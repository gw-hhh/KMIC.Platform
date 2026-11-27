using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>创建示例</summary>
/// <param name="Title">标题</param>
/// <param name="Items">明细</param>
[RequirePermission(SamplePermissions.Create)]
[Audited]
[Idempotent]
public sealed record CreateSpecimenCommand(string Title, IReadOnlyList<CreateSpecimenItemInput> Items) : ICommand<CreatedResponse<SpecimenId>>;
