namespace Spike01.BuildingBlocks;

/// <summary>
/// 原型中代替 FluentValidation 的最小校验接口
/// </summary>
public interface IValidatable
{
    Error? Validate();
}
