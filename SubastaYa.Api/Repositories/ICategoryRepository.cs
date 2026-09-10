using SubastaYa.Api.Dtos.Categories;

namespace SubastaYa.Api.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryDto>> GetAllAsync();
}