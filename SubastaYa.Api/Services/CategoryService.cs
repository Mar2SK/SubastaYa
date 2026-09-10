using SubastaYa.Api.Dtos.Categories;
using SubastaYa.Api.Repositories;

namespace SubastaYa.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }
}