namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 声明用例受功能开关控制，关闭时不执行
/// </summary>
/// <remarks>
/// 开关不授予权限，开启后仍按权限点授权
/// </remarks>
/// <param name="name">开关名，格式 <c>{模块}.{功能}</c>，如 <c>Sample.NewImport</c></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FeatureGateAttribute(string name) : Attribute
{
    /// <summary>
    /// 开关名
    /// </summary>
    public string Name { get; } = name;
}
