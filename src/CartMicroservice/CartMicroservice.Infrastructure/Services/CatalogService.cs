using System.Net.Http.Json;
using CartMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CartMicroservice.Infrastructure.Services;

public class CatalogService : ICatalogService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(HttpClient httpClient, ILogger<CatalogService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _logger.LogInformation("CatalogService initialized with base address: {BaseAddress}", _httpClient.BaseAddress);
    }

    public async Task<CatalogProductInfo?> GetProductInfoAsync(int productId)
    {
        try
        {
            var requestUrl = $"/api/products/{productId}";
            _logger.LogInformation("Making request to {RequestUrl} with base address {BaseAddress}", 
                requestUrl, _httpClient.BaseAddress);
            
            var response = await _httpClient.GetAsync(requestUrl);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Response from catalog service: Status={StatusCode}, Content={Content}, Headers={Headers}", 
                response.StatusCode, 
                content,
                string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get product info. Status code: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                return null;
            }

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found in catalog", productId);
                return null;
            }

            _logger.LogInformation("Successfully retrieved product {ProductId}: {ProductName}", 
                productId, product.Name);
            
            return new CatalogProductInfo(
                product.Id,
                product.Name,
                product.Price,
                product.ImageUrl,
                product.Attributes.ToDictionary(a => a.Key, a => a.Value),
                product.StockQuantity
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while getting product info for product {ProductId}. Inner exception: {InnerException}", 
                productId, ex.InnerException?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product info for product {ProductId}. Inner exception: {InnerException}", 
                productId, ex.InnerException?.Message);
            return null;
        }
    }

    public async Task<IEnumerable<CatalogProductInfo>> GetProductsInfoAsync(IEnumerable<int> productIds)
    {
        try
        {
            _logger.LogInformation("Requesting products info for {ProductIds} from {BaseUrl}", 
                string.Join(", ", productIds), _httpClient.BaseAddress);

            var response = await _httpClient.GetAsync($"/api/products?ids={string.Join(",", productIds)}");
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Received response from catalog service. Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get products info. Status code: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
                return Array.Empty<CatalogProductInfo>();
            }

            var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductResponse>>();
            if (products == null)
            {
                _logger.LogWarning("No products found in catalog for ids: {ProductIds}", string.Join(", ", productIds));
                return Array.Empty<CatalogProductInfo>();
            }

            _logger.LogInformation("Successfully retrieved {Count} products", products.Count());
            return products.Select(p => new CatalogProductInfo(
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
                p.Attributes.ToDictionary(a => a.Key, a => a.Value),
                p.StockQuantity
            ));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while getting products info for {ProductIds}", 
                string.Join(", ", productIds));
            return Array.Empty<CatalogProductInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products info for {ProductIds}", string.Join(", ", productIds));
            return Array.Empty<CatalogProductInfo>();
        }
    }

    private record ProductResponse(
        int Id,
        string Name,
        string? Description,
        decimal Price,
        int StockQuantity,
        int? CategoryId,
        string? ImageUrl,
        IEnumerable<ProductAttributeResponse> Attributes
    );

    private record ProductAttributeResponse(
        string Key,
        string Value
    );
} 