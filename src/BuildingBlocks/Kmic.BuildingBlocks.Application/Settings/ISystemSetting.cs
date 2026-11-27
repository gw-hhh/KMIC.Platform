using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

public interface ISystemSetting<TSelf>
    where TSelf : class, ISystemSetting<TSelf>
{
    static abstract TSelf Default { get; }

    static abstract Result Validate(TSelf value);
}
