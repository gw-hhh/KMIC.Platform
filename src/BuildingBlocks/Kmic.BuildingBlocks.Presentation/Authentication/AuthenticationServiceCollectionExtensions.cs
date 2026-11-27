using Kmic.BuildingBlocks.Application.Access;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>注册 RS256 访问令牌认证与密钥轮换支持（保留 sub、sid、rid 原始声明名）</summary>
    public static IServiceCollection AddKmicAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<KmicAuthOptions>()
            .BindConfiguration(KmicAuthOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<SigningKeyStore>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        // 启动阶段即校验签名密钥配置，避免首个请求到达时才暴露异常
        services.AddTransient<IStartupFilter, SigningKeyValidationFilter>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<KmicAuthOptions>, SigningKeyStore>(Configure);
        services.AddAuthorization();
        return services;
    }

    /// <summary>加载访问上下文、写入授权版本头并拦截不可用角色（须置于路由与认证之后、授权之前）</summary>
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

            // 仅接受 RS256 算法（拒绝 none 与对称算法）；kid 不匹配时不回退尝试其他密钥
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            IssuerSigningKeys = keys.ValidationKeys,
            TryAllIssuerSigningKeys = false,
            NameClaimType = "sub",
        };
    }
}
