using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartMicroservice.Contracts.Messages;
using CartMicroservice.Domain.Entities;
using CartMicroservice.Domain.Interfaces;
using CartMicroservice.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CartMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly ICatalogService _catalogService;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        ICatalogService catalogService,
        IMessageBroker messageBroker,
        ILogger<CartController> logger)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _catalogService = catalogService;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var cart = await GetOrCreateCartAsync();
        var cartDto = await MapToCartDtoAsync(cart);
        return Ok(cartDto);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddItemRequest request)
    {
        var cart = await GetOrCreateCartAsync();
        
        var product = await _catalogService.GetProductInfoAsync(request.ProductId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        var existingItem = await _cartItemRepository.GetByCartAndProductAsync(cart.Id, request.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            await _cartItemRepository.UpdateAsync(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = product.Price
            };
            await _cartItemRepository.CreateAsync(newItem);
        }

        cart = await GetOrCreateCartAsync();
        var cartDto = await MapToCartDtoAsync(cart);
        return Ok(cartDto);
    }

    [HttpPut("items/{productId}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int productId, [FromBody] UpdateItemRequest request)
    {
        var cart = await GetOrCreateCartAsync();
        var item = await _cartItemRepository.GetByCartAndProductAsync(cart.Id, productId);

        if (item == null)
        {
            return NotFound();
        }

        item.Quantity = request.Quantity;
        await _cartItemRepository.UpdateAsync(item);

        cart = await GetOrCreateCartAsync();
        var cartDto = await MapToCartDtoAsync(cart);
        return Ok(cartDto);
    }

    [HttpDelete("items/{productId}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int productId)
    {
        var cart = await GetOrCreateCartAsync();
        var item = await _cartItemRepository.GetByCartAndProductAsync(cart.Id, productId);

        if (item == null)
        {
            return NotFound();
        }

        await _cartItemRepository.DeleteAsync(item.Id);

        cart = await GetOrCreateCartAsync();
        var cartDto = await MapToCartDtoAsync(cart);
        return Ok(cartDto);
    }

    [HttpPost("clear")]
    public async Task<ActionResult<CartDto>> ClearCart()
    {
        var cart = await GetOrCreateCartAsync();
        foreach (var item in cart.Items)
        {
            await _cartItemRepository.DeleteAsync(item.Id);
        }

        cart = await GetOrCreateCartAsync();
        var cartDto = await MapToCartDtoAsync(cart);
        return Ok(cartDto);
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        Cart? cart = null;
        try
        {
            cart = await GetOrCreateCartAsync();
            if (!cart.Items.Any())
            {
                return BadRequest("Cart is empty");
            }

            // Проверяем наличие всех товаров
            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var products = await _catalogService.GetProductsInfoAsync(productIds);
            var productsDict = products.ToDictionary(p => p.Id);

            var insufficientStockItems = new List<(int ProductId, string Name, int Requested, int Available)>();
            foreach (var item in cart.Items)
            {
                if (!productsDict.TryGetValue(item.ProductId, out var product))
                {
                    return BadRequest($"Product with ID {item.ProductId} not found in catalog");
                }

                if (item.Quantity > product.StockQuantity)
                {
                    insufficientStockItems.Add((item.ProductId, product.Name, item.Quantity, product.StockQuantity));
                }
            }

            if (insufficientStockItems.Any())
            {
                var errorMessage = string.Join(", ", insufficientStockItems.Select(i => 
                    $"'{i.Name}': запрошено {i.Requested}, доступно {i.Available}"));
                return BadRequest(new { Error = "Некоторых позиций нет в наличии", Details = errorMessage });
            }

            var userId = Guid.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value!);
            
            var checkoutMessage = new CartCheckoutMessage
            {
                UserId = userId,
                DeliveryAddress = request.DeliveryAddress,
                Items = cart.Items.Select(item => new CartItemMessage
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            try
            {
                await _messageBroker.PublishCartCheckoutAsync(checkoutMessage);
                _logger.LogInformation("Cart checkout message published for user {UserId}", userId);

                // Clear the cart only after successful message publishing
                foreach (var item in cart.Items.ToList())
                {
                    await _cartItemRepository.DeleteAsync(item.Id);
                }

                return Ok(new { Message = "Checkout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish cart checkout message for user {UserId}", userId);
                return StatusCode(500, "Failed to process checkout. Please try again.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during checkout process");
            return StatusCode(500, "Error processing checkout");
        }
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
        Cart? cart = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = Guid.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value!);
            cart = await _cartRepository.GetByUserIdAsync(userId);
        }
        else
        {
            var sessionToken = Request.Cookies["cart_session"];
            if (!string.IsNullOrEmpty(sessionToken))
            {
                cart = await _cartRepository.GetBySessionTokenAsync(sessionToken);
            }
        }

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = User.Identity?.IsAuthenticated == true
                    ? Guid.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value!)
                    : null,
                SessionToken = User.Identity?.IsAuthenticated == true ? null : Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            cart = await _cartRepository.CreateAsync(cart);

            if (!User.Identity?.IsAuthenticated == true)
            {
                Response.Cookies.Append("cart_session", cart.SessionToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                });
            }
        }

        return cart;
    }

    private async Task<CartDto> MapToCartDtoAsync(Cart cart)
    {
        var productIds = cart.Items.Select(i => i.ProductId);
        var products = await _catalogService.GetProductsInfoAsync(productIds);
        var productsDict = products.ToDictionary(p => p.Id);

        var itemDtos = cart.Items.Select(item =>
        {
            var product = productsDict.GetValueOrDefault(item.ProductId);
            return new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                ProductName = product?.Name ?? "Unknown",
                ProductImageUrl = product?.ImageUrl
            };
        }).ToList();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionToken = cart.SessionToken,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            Items = itemDtos,
            TotalPrice = itemDtos.Sum(i => i.Price * i.Quantity)
        };
    }
}

public class CartDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class AddItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateItemRequest
{
    public int Quantity { get; set; }
}

public class CheckoutRequest
{
    public string DeliveryAddress { get; set; } = "";
} 