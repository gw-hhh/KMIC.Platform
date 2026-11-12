using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Errors;

/// <summary>
/// Configuration 模块的错误
/// </summary>
public static class ConfigurationErrors
{
    /// <summary>
    /// 字典类型不存在
    /// </summary>
    public static readonly Error DictionaryTypeNotFound = Error.NotFound("Configuration.DictionaryTypeNotFound", "字典类型不存在");

    /// <summary>
    /// 字典类型编码已经登记过
    /// </summary>
    public static readonly Error DictionaryTypeCodeExists = Error.Conflict("Configuration.DictionaryTypeCodeExists", "这个字典类型已经登记过");

    /// <summary>
    /// 字典类型已经从代码里删除，不能再改它的字典项
    /// </summary>
    public static readonly Error DictionaryTypeRetired = Error.BusinessRule("Configuration.DictionaryTypeRetired", "这个字典类型已经不再使用，不能修改");

    /// <summary>
    /// 字典项不存在
    /// </summary>
    public static readonly Error DictionaryItemNotFound = Error.NotFound("Configuration.DictionaryItemNotFound", "字典项不存在");

    /// <summary>
    /// 同一字典类型下已经有这个编码
    /// </summary>
    public static readonly Error DictionaryItemCodeExists = Error.Conflict("Configuration.DictionaryItemCodeExists", "这个字典类型下已经有同样编码的字典项");

    /// <summary>
    /// 字典项编码的写法不对
    /// </summary>
    public static readonly Error DictionaryItemCodeInvalid = Error.BusinessRule("Configuration.DictionaryItemCodeInvalid", "字典项编码只能用字母、数字、点、短横线、下划线，长度 1 到 50");

    /// <summary>
    /// 字典项名称不能为空
    /// </summary>
    public static readonly Error DictionaryItemNameInvalid = Error.Validation("Configuration.DictionaryItemNameInvalid", "字典项名称不能为空，最长 200 个字符");

    /// <summary>
    /// 父项不存在、不是启用的、字典不分层，或者会让字典项成环
    /// </summary>
    public static readonly Error DictionaryParentInvalid = Error.BusinessRule("Configuration.DictionaryParentInvalid", "父项不存在或已停用，字典类型不分层，或者会让字典项成环");

    /// <summary>
    /// 下面还有启用的子项，不能停用
    /// </summary>
    public static readonly Error DictionaryItemHasEnabledChildren = Error.BusinessRule("Configuration.DictionaryItemHasEnabledChildren", "下面还有启用的子项，请先停用它们");

    /// <summary>
    /// 字典项太多
    /// </summary>
    public static readonly Error DictionaryTooManyItems = Error.BusinessRule("Configuration.DictionaryTooManyItems", "这个字典类型的字典项已经达到上限");

    /// <summary>
    /// 调整次序时没有给全或给多了
    /// </summary>
    public static readonly Error DictionaryOrderInvalid = Error.BusinessRule("Configuration.DictionaryOrderInvalid", "排序必须包含同一父项下的全部字典项，且不能重复");

    /// <summary>
    /// 状态不允许这个操作
    /// </summary>
    public static readonly Error InvalidState = Error.BusinessRule("Configuration.InvalidState", "当前状态不能执行这个操作");

    /// <summary>
    /// 设置不存在
    /// </summary>
    public static readonly Error SettingNotFound = Error.NotFound("Configuration.SettingNotFound", "设置不存在");

    /// <summary>
    /// 设置的值不合法
    /// </summary>
    public static readonly Error SettingInvalid = Error.BusinessRule("Configuration.SettingInvalid", "设置的值不合法");

    /// <summary>
    /// 偏好不存在
    /// </summary>
    public static readonly Error PreferenceNotFound = Error.NotFound("Configuration.PreferenceNotFound", "偏好不存在");

    /// <summary>
    /// 这个偏好不允许在这一层设置
    /// </summary>
    public static readonly Error PreferenceLayerNotAllowed = Error.BusinessRule("Configuration.PreferenceLayerNotAllowed", "这个偏好不允许在这一层设置");

    /// <summary>
    /// 偏好的值不合法
    /// </summary>
    public static readonly Error PreferenceInvalid = Error.BusinessRule("Configuration.PreferenceInvalid", "偏好的值不合法");

    /// <summary>
    /// 给偏好指定的角色不存在
    /// </summary>
    public static readonly Error PreferenceRoleNotFound = Error.NotFound("Configuration.PreferenceRoleNotFound", "角色不存在");

    /// <summary>
    /// 设置的值通不过设置自己的校验：逐字段的错误随响应带回，界面按成员名放回对应的输入框
    /// </summary>
    public static Error SettingInvalidWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(SettingInvalid.Code, SettingInvalid.Message, fieldErrors);

    /// <summary>
    /// 偏好的值通不过偏好自己的校验：逐字段的错误随响应带回
    /// </summary>
    public static Error PreferenceInvalidWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(PreferenceInvalid.Code, PreferenceInvalid.Message, fieldErrors);
}
