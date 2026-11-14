using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.GetDetail;

/// <summary>
/// 查询示例的详情
/// </summary>
[RequirePermission(SamplePermissions.View)]
public sealed record GetSpecimenDetailQuery(SpecimenId Id) : IQuery<SpecimenDetailResponse>;
