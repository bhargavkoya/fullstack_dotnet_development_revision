using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using OrderApi.BlazorClient.Auth;

namespace OrderApi.BlazorClient.HttpClients;

/// <summary>[Pattern: Decorator][SRP] Decorates outgoing HTTP requests with auth token and handles 401 refresh/retry flow.</summary>
public sealed class AuthenticatedHttpHandler : DelegatingHandler
{
    private const string RetryHeaderName = "X-Retry-Attempt";
    private const string LoginPath = "/login";

    private readonly TokenManager _tokenManager;
    private readonly JwtAuthStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;

    /// <summary>
    /// [OCP] Hook for silent refresh orchestration configured by auth service in a later step.
    /// Return true when refresh succeeds and tokens are persisted.
    /// </summary>
    public Func<Task<bool>>? TryRefreshTokensAsync { get; set; }

    public AuthenticatedHttpHandler(
        TokenManager tokenManager,
        JwtAuthStateProvider authStateProvider,
        NavigationManager navigation)
    {
        _tokenManager = tokenManager;
        _authStateProvider = authStateProvider;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AttachBearerTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || HasRetryAttempt(request))
        {
            return response;
        }

        response.Dispose();

        var refreshed = await TryRefreshAsync();
        if (!refreshed)
        {
            await _authStateProvider.MarkUserAsLoggedOut();
            _navigation.NavigateTo(LoginPath, forceLoad: false);
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = request
            };
        }

        var retriedRequest = await CloneRequestAsync(request);
        MarkRetryAttempt(retriedRequest);
        await AttachBearerTokenAsync(retriedRequest);

        return await base.SendAsync(retriedRequest, cancellationToken);
    }

    private async Task AttachBearerTokenAsync(HttpRequestMessage request)
    {
        var token = await _tokenManager.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<bool> TryRefreshAsync()
    {
        if (TryRefreshTokensAsync is null)
        {
            return false;
        }

        try
        {
            return await TryRefreshTokensAsync.Invoke();
        }
        catch
        {
            return false;
        }
    }

    private static bool HasRetryAttempt(HttpRequestMessage request) =>
        request.Headers.Contains(RetryHeaderName);

    private static void MarkRetryAttempt(HttpRequestMessage request) =>
        request.Headers.TryAddWithoutValidation(RetryHeaderName, "1");

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
}
