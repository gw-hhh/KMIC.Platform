using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 认证的服务注册与中间件
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// 注册访问令牌的验证：JwtBearer，RS256，签名密钥可轮换
    /// </summary>
    /// <remarks>
    /// 配置在 Kmic:Auth。令牌里的声明保持原名（sub、sid、rid），不映射成 .NET 的声明类型
    /// </remarks>
    public static IServiceCollection AddKmicAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<KmicAuthOptions>()
            .BindConfiguration(KmicAuthOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<SigningKeyStore>();

        // 密钥有问题时在启动阶段就失败，而不是第一个请求才发现
        services.AddTransient<IStartupFilter, SigningKeyValidationFilter>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<KmicAuthOptions>, SigningKeyStore>(Configure);
        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// 为每个请求加载访问上下文、设置当前身份、写授权版本头，并在当前角色不可用时拦截
    /// </summary>
    /// <remarks>
    /// 放在认证之后、授权之前，并且在路由之后
    /// </remarks>
    public static IApplicationBuilder UseKmicAccessContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<AccessContextMiddleware>();
    }

    private static void Configure(JwtBearerOptions options, IOptions<KmicAuthOptions> auth, SigningKeyStore keys)
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = auth.Value.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = auth.Value.ClockSkew,

            // 只接受 RS256：拒绝 none 和对称算法；kid 对不上的令牌不去试别的密钥
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            IssuerSigningKeys = keys.ValidationKeys,
            TryAllIssuerSigningKeys = false,
            NameClaimType = "sub",
        };
    }
}
