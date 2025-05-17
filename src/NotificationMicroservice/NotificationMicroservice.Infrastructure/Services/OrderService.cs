using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationMicroservice.Application.Services;

namespace NotificationMicroservice.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OrderService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OrderInfo> GetOrderInfoAsync(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Getting order info for order {OrderId} from {BaseAddress}", 
                    orderId, _httpClient.BaseAddress);

                var response = await _httpClient.GetAsync($"/api/orders/{orderId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get order info for order {OrderId}. Status code: {StatusCode}", 
                        orderId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received order info: {Content}", content);

                var orderInfo = JsonSerializer.Deserialize<OrderInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (orderInfo == null)
                {
                    _logger.LogWarning("Failed to deserialize order info for order {OrderId}", orderId);
                    return null;
                }

                _logger.LogInformation("Successfully got order info for order {OrderId}, user {UserId}", 
                    orderId, orderInfo.UserId);
                return orderInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order info for order {OrderId}", orderId);
                return null;
            }
        }
    }
} 