using CatalogMicroservice.Contracts.DTOs;
using CatalogMicroservice.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public CategoriesController(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories.Select(c => new CategoryResponse(
            c.Id,
            c.Name,
            c.ParentId
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(new CategoryResponse(
            category.Id,
            category.Name,
            category.ParentId
        ));
    }

    [HttpGet("{id}/products")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        var products = await _productRepository.GetByCategoryIdAsync(id);
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