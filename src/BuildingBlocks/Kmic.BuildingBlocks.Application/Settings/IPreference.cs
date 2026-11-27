using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>用户偏好契约；仅用于个人使用体验偏好，不得用于权限控制。</summary>
public interface IPreference<TSelf>
    where TSelf : class, IPreference<TSelf>
{
    static abstract TSelf Default { get; }

    static abstract Result Validate(TSelf value);
}
