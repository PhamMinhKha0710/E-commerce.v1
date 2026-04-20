using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using E_commerce.v1.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Infrastructure.Shipping;

public class AhamoveClient : IAhamoveClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonRead;
    private readonly JsonSerializerOptions _jsonWrite;

    public AhamoveClient(HttpClient http, IOptions<AhamoveOptions> options)
    {
        _http = http;
        var o = options.Value;
        _jsonRead = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        _jsonWrite = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var baseUrl = o.BaseUrl?.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(baseUrl))
            _http.BaseAddress = new Uri(baseUrl + "/");

        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(o.BearerToken))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                o.BearerToken.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(o.ApiKey))
        {
            _http.DefaultRequestHeaders.TryAddWithoutValidation("apikey", o.ApiKey.Trim());
        }

        var timeoutSec = o.TimeoutSeconds > 0 ? o.TimeoutSeconds : 60;
        _http.Timeout = TimeSpan.FromSeconds(timeoutSec);
    }

    public async Task<IReadOnlyList<AhamoveEstimateResultItem>> EstimateAsync(
        AhamoveEstimateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("v3/orders/estimates", request, _jsonWrite, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var list = await JsonSerializer.DeserializeAsync<List<AhamoveEstimateResultItem>>(stream, _jsonRead, cancellationToken);
        return list ?? new List<AhamoveEstimateResultItem>();
    }

    public async Task<AhamoveCreateOrderResponse> CreateOrderAsync(
        AhamoveCreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("v3/orders", request, _jsonWrite, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var doc = await JsonSerializer.DeserializeAsync<JsonElement>(stream, _jsonRead, cancellationToken);
        var orderId = doc.TryGetProperty("order_id", out var oid) ? oid.GetString() ?? string.Empty : string.Empty;
        var status = doc.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty;
        string? shared = null;
        if (doc.TryGetProperty("shared_link", out var sl))
            shared = sl.GetString();

        return new AhamoveCreateOrderResponse
        {
            OrderId = orderId,
            Status = status,
            SharedLink = shared
        };
    }

    public async Task<AhamoveOrderDetailsResponse> GetOrderDetailsAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync($"v3/orders/{orderId}", cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<AhamoveOrderDetailsResponse>(stream, _jsonRead, cancellationToken);
        return result ?? new AhamoveOrderDetailsResponse();
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new BadRequestException(
            string.IsNullOrWhiteSpace(body)
                ? $"Ahamove API lỗi: {(int)response.StatusCode} {response.ReasonPhrase}"
                : $"Ahamove API lỗi: {(int)response.StatusCode} — {body}");
    }
}
