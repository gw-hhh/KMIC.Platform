using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>
/// 创建示例
/// </summary>
public sealed class CreateSpecimenCommandHandler(ISpecimenRepository specimens, INumberGenerator numbers, IExecutionContext execution)
    : ICommandHandler<CreateSpecimenCommand, CreatedResponse<SpecimenId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CreatedResponse<SpecimenId>>> Handle(CreateSpecimenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 必须明确所属组织，才能按组织限制数据访问范围
        var access = execution.Access ?? throw new InvalidOperationException("创建示例需要用户身份。");
        if (access.OrgUnitId is not { } orgUnit)
        {
            return SampleErrors.NoOrgUnit;
        }

        var number = await numbers.NextAsync(SampleNumberRules.Specimen, ct: cancellationToken);
        var specimen = Specimen.Create(number, command.Title, access.UserId, orgUnit);
        foreach (var item in command.Items)
        {
            var added = specimen.AddItem(item.Name, item.Quantity);
            if (added.IsFailure)
            {
                return added.Error;
            }
        }

        specimens.Add(specimen);
        return new CreatedResponse<SpecimenId>(specimen.Id);
    }
}
