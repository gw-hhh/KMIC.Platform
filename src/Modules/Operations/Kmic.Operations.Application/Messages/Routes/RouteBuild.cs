namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 声明订阅的构建信息
/// </summary>
/// <param name="Build">构建号</param>
/// <param name="Instances">声明该订阅的存活实例数</param>
public sealed record RouteBuild(string Build, int Instances);
