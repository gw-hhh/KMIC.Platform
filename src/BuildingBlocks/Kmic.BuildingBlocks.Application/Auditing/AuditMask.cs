namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>审计字段脱敏遮盖方式。</summary>
public enum AuditMask
{
    /// <summary>手机号，保留前 3 位和后 4 位。</summary>
    Phone,

    /// <summary>邮箱，保留第 1 个字符和域名。</summary>
    Email,

    /// <summary>证件号，保留前 3 位和后 4 位。</summary>
    IdNumber,

    /// <summary>按内容脱敏，供 Personal 分类数据使用。</summary>
    Personal,
}
