using System.Text.Json;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.HttpClients;

/// <summary>[Pattern: Template Method][SRP] Defines a reusable API call pipeline: execute, validate, deserialize.</summary>
public abstract class ApiClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected readonly HttpClient Http;

    protected ApiClientBase(HttpClient http)
    {
        Http = http;
    }

    /// <summary>
    /// [Pattern: Template Method] Fixed API execution skeleton while endpoint calls vary in derived clients.
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<HttpResponseMessage>> apiCall)
    {
        var response = await apiCall();
        await HandleErrorResponseAsync(response);
        return await DeserializeAsync<T>(response);
    }

    /// <summary>[OCP] Overridable error mapping for specialized clients.</summary>
    protected virtual async Task HandleErrorResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var parsedError = DeserializeError(content);

        throw new ApiException(
            (int)response.StatusCode,
            parsedError?.Message ?? $"API request failed with status code {(int)response.StatusCode}.");
    }

    /// <summary>[OCP] Overridable deserialization for custom formats.</summary>
    protected virtual async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        if (response.Content is null)
        {
            return default;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default;
        }

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private static ApiErrorResponse? DeserializeError(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptions);
        }
        catch
        {
            return new ApiErrorResponse(content);
        }
    }
}
