using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>将数据分类标为个人数据（如手机号、邮箱、证件号、地址），日志中将部分脱敏。</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PersonalDataAttribute() : DataClassificationAttribute(KmicData.Personal);
