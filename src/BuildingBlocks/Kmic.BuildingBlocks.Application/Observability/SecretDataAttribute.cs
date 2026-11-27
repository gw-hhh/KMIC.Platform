using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>将数据分类标为敏感凭据（如密码、令牌、密钥、验证码），日志中将替换为 ***。</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class SecretDataAttribute() : DataClassificationAttribute(KmicData.Secret);
