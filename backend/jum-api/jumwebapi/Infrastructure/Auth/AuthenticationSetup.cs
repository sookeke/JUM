using jumwebapi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace jumwebapi.Infrastructure.Auth
{
    public static class AuthenticationSetup
    {
        //public IConfiguration Configuration { get; }
        public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, jumwebapiConfiguration config)
        {
            //Configuration = configuration;
            services.ThrowIfNull(nameof(services));
            config.ThrowIfNull(nameof(config));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = config.Keycloak.RealmUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = Resources.JumApi;
                options.MetadataAddress = config.Keycloak.WellKnownConfig;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidAlgorithms = new List<string>() { "RS256" }
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context => {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.OnStarting(async () =>
                        {
                            context.NoResult();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            string response =
                            JsonConvert.SerializeObject("The access token provided is not valid.");
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                                response =
                                    JsonConvert.SerializeObject("The access token provided has expired.");
                            }
                            await context.Response.WriteAsync(response);
                        });
                   
                        //context.HandleResponse();
                        //context.Response.WriteAsync(response).Wait();
                        return Task.CompletedTask;

                    },
                    OnForbidden = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", policy => policy.Requirements.Add(new RealmAccessRoleRequirement("administrator")));
            });
            return services;
            
        }
    }

}
