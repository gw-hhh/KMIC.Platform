using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Domain.Errors;

public static class ConfigurationErrors
{
    public static readonly Error DictionaryTypeNotFound = Error.NotFound("Configuration.DictionaryTypeNotFound", "字典类型不存在");

    public static readonly Error DictionaryTypeCodeExists = Error.Conflict("Configuration.DictionaryTypeCodeExists", "字典类型编码已存在");

    public static readonly Error DictionaryTypeRetired = Error.BusinessRule("Configuration.DictionaryTypeRetired", "字典类型已废弃，不可修改字典项");

    public static readonly Error DictionaryItemNotFound = Error.NotFound("Configuration.DictionaryItemNotFound", "字典项不存在");

    public static readonly Error DictionaryItemCodeExists = Error.Conflict("Configuration.DictionaryItemCodeExists", "该字典类型下已存在相同编码的字典项");

    public static readonly Error DictionaryItemCodeInvalid = Error.BusinessRule("Configuration.DictionaryItemCodeInvalid", "字典项编码须为 1～50 个字符，仅支持字母、数字、点、短横线和下划线");

    public static readonly Error DictionaryItemNameInvalid = Error.Validation("Configuration.DictionaryItemNameInvalid", "字典项名称不能为空，最长 200 个字符");

    public static readonly Error DictionaryParentInvalid = Error.BusinessRule("Configuration.DictionaryParentInvalid", "父项不存在或已停用、字典不支持层级，或父子关系会形成循环");

    public static readonly Error DictionaryItemHasEnabledChildren = Error.BusinessRule("Configuration.DictionaryItemHasEnabledChildren", "仍有启用的子项，请先停用子项");

    public static readonly Error DictionaryTooManyItems = Error.BusinessRule("Configuration.DictionaryTooManyItems", "该字典类型的字典项数量已达到上限");

    public static readonly Error DictionaryOrderInvalid = Error.BusinessRule("Configuration.DictionaryOrderInvalid", "排序必须包含同一父项下的全部字典项，且不能重复");

    public static readonly Error InvalidState = Error.BusinessRule("Configuration.InvalidState", "当前状态不允许此操作");

    public static readonly Error SettingNotFound = Error.NotFound("Configuration.SettingNotFound", "设置不存在");

    public static readonly Error SettingInvalid = Error.BusinessRule("Configuration.SettingInvalid", "设置的值不合法");

    public static readonly Error PreferenceNotFound = Error.NotFound("Configuration.PreferenceNotFound", "偏好不存在");

    public static readonly Error PreferenceLayerNotAllowed = Error.BusinessRule("Configuration.PreferenceLayerNotAllowed", "此偏好不允许在当前层级设置");

    public static readonly Error PreferenceInvalid = Error.BusinessRule("Configuration.PreferenceInvalid", "偏好的值不合法");

    public static readonly Error PreferenceRoleNotFound = Error.NotFound("Configuration.PreferenceRoleNotFound", "角色不存在");

    /// <summary>返回附带字段错误详情的设置校验失败</summary>
    public static Error SettingInvalidWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(SettingInvalid.Code, SettingInvalid.Message, fieldErrors);

    /// <summary>返回附带字段错误详情的偏好校验失败</summary>
    public static Error PreferenceInvalidWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(PreferenceInvalid.Code, PreferenceInvalid.Message, fieldErrors);
}
