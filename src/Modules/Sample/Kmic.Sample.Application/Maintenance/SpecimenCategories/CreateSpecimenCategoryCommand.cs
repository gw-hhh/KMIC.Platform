using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.SpecimenCategories;

namespace Kmic.Sample.Application.Maintenance.SpecimenCategories;

/// <summary>新建示例分类</summary>
/// <param name="Name">名称</param>
[RequirePermission(SamplePermissions.CategoryManage)]
[Audited]
[Idempotent]
public sealed record CreateSpecimenCategoryCommand(string Name) : ICommand<CreatedResponse<SpecimenCategoryId>>;

internal sealed class CreateSpecimenCategoryCommandValidator : AbstractValidator<CreateSpecimenCategoryCommand>
{
    public CreateSpecimenCategoryCommandValidator() => RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
}

public sealed class CreateSpecimenCategoryCommandHandler(IRepository<SpecimenCategory, SpecimenCategoryId> categories)
    : ICommandHandler<CreateSpecimenCategoryCommand, CreatedResponse<SpecimenCategoryId>>
{
    public ValueTask<Result<CreatedResponse<SpecimenCategoryId>>> Handle(CreateSpecimenCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = SpecimenCategory.Create(command.Name);
        categories.Add(category);
        return ValueTask.FromResult<Result<CreatedResponse<SpecimenCategoryId>>>(new CreatedResponse<SpecimenCategoryId>(category.Id));
    }
}
