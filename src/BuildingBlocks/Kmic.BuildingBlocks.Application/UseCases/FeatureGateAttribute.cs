namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>声明用例受功能开关控制（开关名格式 <c>{模块}.{功能}</c>，如 <c>Sample.NewImport</c>）；关闭时不执行，开启后仍按权限点授权。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FeatureGateAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
