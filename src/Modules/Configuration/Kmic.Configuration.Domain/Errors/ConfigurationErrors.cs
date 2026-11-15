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
    /// 字典类型编码已存在
    /// </summary>
    public static readonly Error DictionaryTypeCodeExists = Error.Conflict("Configuration.DictionaryTypeCodeExists", "字典类型编码已存在");

    /// <summary>
    /// 字典类型已从代码中移除，不能修改其字典项
    /// </summary>
    public static readonly Error DictionaryTypeRetired = Error.BusinessRule("Configuration.DictionaryTypeRetired", "字典类型已退役，不可修改字典项");

    /// <summary>
    /// 字典项不存在
    /// </summary>
    public static readonly Error DictionaryItemNotFound = Error.NotFound("Configuration.DictionaryItemNotFound", "字典项不存在");

    /// <summary>
    /// 同一字典类型下已有相同编码
    /// </summary>
    public static readonly Error DictionaryItemCodeExists = Error.Conflict("Configuration.DictionaryItemCodeExists", "该字典类型下已存在相同编码的字典项");

    /// <summary>
    /// 字典项编码格式无效
    /// </summary>
    public static readonly Error DictionaryItemCodeInvalid = Error.BusinessRule("Configuration.DictionaryItemCodeInvalid", "字典项编码须为 1～50 个字符，仅支持字母、数字、点、短横线和下划线");

    /// <summary>
    /// 字典项名称不能为空
    /// </summary>
    public static readonly Error DictionaryItemNameInvalid = Error.Validation("Configuration.DictionaryItemNameInvalid", "字典项名称不能为空，最长 200 个字符");

    /// <summary>
    /// 父项不存在或已停用、字典不支持层级，或父子关系会形成循环
    /// </summary>
    public static readonly Error DictionaryParentInvalid = Error.BusinessRule("Configuration.DictionaryParentInvalid", "父项不存在或已停用、字典不支持层级，或父子关系会形成循环");

    /// <summary>
    /// 存在启用的子项，不能停用
    /// </summary>
    public static readonly Error DictionaryItemHasEnabledChildren = Error.BusinessRule("Configuration.DictionaryItemHasEnabledChildren", "仍有启用的子项，请先停用子项");

    /// <summary>
    /// 字典项数量达到上限
    /// </summary>
    public static readonly Error DictionaryTooManyItems = Error.BusinessRule("Configuration.DictionaryTooManyItems", "该字典类型的字典项数量已达到上限");

    /// <summary>
    /// 排序未包含全部同级字典项或包含重复项
    /// </summary>
    public static readonly Error DictionaryOrderInvalid = Error.BusinessRule("Configuration.DictionaryOrderInvalid", "排序必须包含同一父项下的全部字典项，且不能重复");

    /// <summary>
    /// 状态不允许当前操作
    /// </summary>
    public static readonly Error InvalidState = Error.BusinessRule("Configuration.InvalidState", "当前状态不允许此操作");

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
    public static readonly Error PreferenceLayerNotAllowed = Error.BusinessRule("Configuration.PreferenceLayerNotAllowed", "此偏好不允许在当前层级设置");

    /// <summary>
    /// 偏好的值不合法
    /// </summary>
    public static readonly Error PreferenceInvalid = Error.BusinessRule("Configuration.PreferenceInvalid", "偏好的值不合法");

    /// <summary>
    /// 指定角色不存在
    /// </summary>
    public static readonly Error PreferenceRoleNotFound = Error.NotFound("Configuration.PreferenceRoleNotFound", "角色不存在");

    /// <summary>
    /// 返回设置校验失败及字段错误，供界面定位对应输入项
    /// </summary>
    public static Error SettingInvalidWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(SettingInvalid.Code, SettingInvalid.Message, fieldErrors);

    /// <summary>
    /// 返回偏好校验失败及字段错误
    /// </summary>
    public static Error PreferenceInvalidWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(PreferenceInvalid.Code, PreferenceInvalid.Message, fieldErrors);
}
