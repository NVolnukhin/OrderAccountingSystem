using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Application.Interfaces;

namespace OrderMicroservice.Infrastructure.Services;

public class CatalogService : ICatalogService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(HttpClient httpClient, IConfiguration configuration, ILogger<CatalogService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(configuration["Services:Catalog"] ?? "http://localhost:5080");
    }

    public async Task<CatalogProductInfo?> GetProductInfoAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Products/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get product info for ID {ProductId}. Status code: {StatusCode}", 
                    productId, response.StatusCode);
                return null;
            }

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found in catalog", productId);
                return null;
            }

            var attributes = new Dictionary<string, string>();
            if (product.Attributes.HasValue)
            {
                try
                {
                    if (product.Attributes.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in product.Attributes.Value.EnumerateObject())
                        {
                            attributes[property.Name] = property.Value.GetString() ?? string.Empty;
                        }
                    }
                    else if (product.Attributes.Value.ValueKind == JsonValueKind.Array)
                    {
                        var index = 0;
                        foreach (var item in product.Attributes.Value.EnumerateArray())
                        {
                            attributes[$"item_{index}"] = item.GetString() ?? string.Empty;
                            index++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing attributes for product {ProductId}", product.Id);
                }
            }

            return new CatalogProductInfo(
                product.Id,
                product.Name,
                product.Price,
                product.ImageUrl,
                attributes,
                product.StockQuantity
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product info for ID: {ProductId}", productId);
            return null;
        }
    }

    public async Task<IEnumerable<CatalogProductInfo>> GetProductsInfoAsync(IEnumerable<int> productIds)
    {
        try
        {
            var ids = string.Join(",", productIds);
            var response = await _httpClient.GetAsync($"/api/Products?ids={ids}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get products info. Status code: {StatusCode}", response.StatusCode);
                return Array.Empty<CatalogProductInfo>();
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
            if (products == null)
            {
                _logger.LogWarning("No products found in catalog for IDs: {ProductIds}", ids);
                return Array.Empty<CatalogProductInfo>();
            }

            return products.Select(p => 
            {
                var attributes = new Dictionary<string, string>();
                if (p.Attributes.HasValue)
                {
                    try
                    {
                        var element = p.Attributes.Value;
                        if (element.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var property in element.EnumerateObject())
                            {
                                if (property.Value.ValueKind == JsonValueKind.String)
                                {
                                    attributes[property.Name] = property.Value.GetString() ?? string.Empty;
                                }
                                else if (property.Value.ValueKind == JsonValueKind.Number)
                                {
                                    attributes[property.Name] = property.Value.GetRawText();
                                }
                                else if (property.Value.ValueKind == JsonValueKind.True || 
                                       property.Value.ValueKind == JsonValueKind.False)
                                {
                                    attributes[property.Name] = property.Value.GetBoolean().ToString();
                                }
                                else if (property.Value.ValueKind == JsonValueKind.Object)
                                {
                                    attributes[property.Name] = property.Value.GetRawText();
                                }
                            }
                        }
                        else if (element.ValueKind == JsonValueKind.Array)
                        {
                            var index = 0;
                            foreach (var item in element.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String)
                                {
                                    attributes[$"item_{index}"] = item.GetString() ?? string.Empty;
                                }
                                else
                                {
                                    attributes[$"item_{index}"] = item.GetRawText();
                                }
                                index++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing attributes for product {ProductId}", p.Id);
                    }
                }

                return new CatalogProductInfo(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.ImageUrl,
                    attributes,
                    p.StockQuantity
                );
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products info for IDs: {ProductIds}", string.Join(", ", productIds));
            return Array.Empty<CatalogProductInfo>();
        }
    }

    private class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public JsonElement? Attributes { get; set; }
        public int StockQuantity { get; set; }
    }
} 