using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>
/// 处理 <see cref="CreateSpecimenCommand"/>
/// </summary>
public sealed class CreateSpecimenCommandHandler(ISpecimenRepository specimens, INumberGenerator numbers, IExecutionContext execution)
    : ICommandHandler<CreateSpecimenCommand, CreatedResponse<SpecimenId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CreatedResponse<SpecimenId>>> Handle(CreateSpecimenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 授权已经保证有登录用户；所属组织是数据范围的依据，没有就无法确定数据归谁
        var access = execution.Access ?? throw new InvalidOperationException("创建示例标本要求用户身份。");
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
