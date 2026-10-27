using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.API.Auth;

public static class SupabaseJwtAuth
{
    public static IServiceCollection AddSupabaseAuth(this IServiceCollection services, IConfiguration config)
    {
        var supabaseUrl = config["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url not configured");
        var projectUrl = supabaseUrl.Replace("/auth/v1", "");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{projectUrl}/auth/v1";
                options.MetadataAddress = $"{projectUrl}/auth/v1/.well-known/openid-configuration";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = $"{projectUrl}/auth/v1",
                    ValidateAudience = true,
                    ValidAudience = "authenticated",
                    ValidateLifetime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
