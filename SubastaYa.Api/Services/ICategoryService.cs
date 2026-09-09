using SubastaYa.Api.Dtos.Categories;

namespace SubastaYa.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
}