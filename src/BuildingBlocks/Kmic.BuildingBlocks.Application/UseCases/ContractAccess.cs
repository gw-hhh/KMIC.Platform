namespace Kmic.BuildingBlocks.Application.UseCases;

public enum ContractAccess
{
    /// <summary>引用解析：跳过权限点与数据权限校验，仅限返回 ReferenceInfo。</summary>
    Reference,

    /// <summary>不检查权限点，按调用方的数据权限过滤。</summary>
    CallerScoped,

    /// <summary>只允许系统身份调用。</summary>
    SystemOnly,
}
