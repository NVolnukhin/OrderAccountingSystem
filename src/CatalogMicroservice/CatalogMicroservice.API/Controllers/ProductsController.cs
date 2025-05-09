using CatalogMicroservice.Contracts.DTOs;
using CatalogMicroservice.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.CategoryId,
            p.ImageUrl,
            p.Attributes.Select(a => new ProductAttributeResponse(a.Key, a.Value))
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CategoryId,
            product.ImageUrl,
            product.Attributes.Select(a => new ProductAttributeResponse(a.Key, a.Value))
        ));
    }

    [HttpPost("details")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetByIds([FromBody] GetProductsByIdsRequest request)
    {
        var products = await _productRepository.GetByIdsAsync(request.Ids);
        return Ok(products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.CategoryId,
            p.ImageUrl,
            p.Attributes.Select(a => new ProductAttributeResponse(a.Key, a.Value))
        )));
    }
} 