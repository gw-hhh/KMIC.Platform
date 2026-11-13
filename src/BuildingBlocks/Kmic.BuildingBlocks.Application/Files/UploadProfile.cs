namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 校验过的上传档案
/// </summary>
/// <param name="Name">档案名</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Module">声明它的模块</param>
/// <param name="MaxBytes">单个文件最大字节数</param>
/// <param name="AllowedTypes">允许的文件类型编码</param>
/// <param name="Permission">上传要有的权限点</param>
public sealed record UploadProfile(string Name, string DisplayName, string Module, long MaxBytes, IReadOnlyList<string> AllowedTypes, string Permission)
{
    /// <summary>
    /// 这种类型这个档案允许不允许
    /// </summary>
    /// <param name="code">类型编码</param>
    public bool Allows(string code) => AllowedTypes.Contains(code, StringComparer.Ordinal);
}
