namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>允许匿名访问（仅限 Identity 模块使用）。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowAnonymousAttribute : Attribute;
