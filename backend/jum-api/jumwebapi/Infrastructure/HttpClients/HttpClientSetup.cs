namespace jumwebapi.Infrastructure.HttpClients;

using IdentityModel.Client;

using jumwebapi.Infrastructure.Auth;
using jumwebapi.Infrastructure.HttpClients.Keycloak;
using jumwebapi.Infrastructure.HttpClients.Mail;
using jumwebapi.Extensions;
using jumwebapi.Infrastructure.HttpClients.JustinParticipant;

public static class HttpClientSetup
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, jumwebapiConfiguration config)
    {
        services.AddHttpClient<IAccessTokenClient, AccessTokenClient>();

        services.AddHttpClientWithBaseAddress<IChesClient, ChesClient>(config.ChesClient.Url)
            .WithBearerToken(new ChesClientCredentials
            {
                Address = config.ChesClient.TokenUrl,
                ClientId = config.ChesClient.ClientId,
                ClientSecret = config.ChesClient.ClientSecret
            });

        services.AddHttpClientWithBaseAddress<IKeycloakAdministrationClient, KeycloakAdministrationClient>(config.Keycloak.AdministrationUrl)
            .WithBearerToken(new KeycloakAdministrationClientCredentials
            {
                Address = config.Keycloak.TokenUrl,
                ClientId = config.Keycloak.AdministrationClientId,
                ClientSecret = config.Keycloak.AdministrationClientSecret
            });

        if (string.IsNullOrEmpty(config.JustinParticipantClient.ApiKey))
        {
            services.AddHttpClientWithBaseAddress<IJustinParticipantClient, JustinParticipantClient>(config.JustinParticipantClient.Url);
        } else
        {
            services.AddHttpClientWithBaseAddress<IJustinParticipantClient, JustinParticipantClient>(config.JustinParticipantClient.Url)
                .ConfigureHttpClient(client => client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.JustinParticipantClient.ApiKey));
        }

        services.AddTransient<ISmtpEmailClient, SmtpEmailClient>();

        return services;
    }

    public static IHttpClientBuilder AddHttpClientWithBaseAddress<TClient, TImplementation>(this IServiceCollection services, string baseAddress)
        where TClient : class
        where TImplementation : class, TClient
        => services.AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = new Uri(baseAddress.EnsureTrailingSlash()));

    public static IHttpClientBuilder WithBearerToken<T>(this IHttpClientBuilder builder, T credentials) where T : ClientCredentialsTokenRequest
    {
        builder.Services.AddSingleton(credentials)
            .AddTransient<BearerTokenHandler<T>>();

        builder.AddHttpMessageHandler<BearerTokenHandler<T>>();

        return builder;
    }
}
