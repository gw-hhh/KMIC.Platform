using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 标注属性、字段或参数的数据分类：密码、令牌、密钥、验证码，日志里输出 ***
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class SecretDataAttribute() : DataClassificationAttribute(KmicData.Secret);
