using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Dtos.Categories;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        List<CategoryDto> categories =
            await _categoryService.GetAllAsync();

        return Ok(categories);
    }
}