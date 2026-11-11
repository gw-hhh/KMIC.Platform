namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 声明不再使用的订阅
/// </summary>
/// <param name="Name">订阅名</param>
/// <param name="Module">订阅方模块</param>
/// <param name="Reason">原因</param>
internal sealed record RetiredSubscription(string Name, string Module, string Reason);
