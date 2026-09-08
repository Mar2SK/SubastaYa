namespace SubastaYa.Api.Dtos;

public class PagedResponseDto<T>
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public List<T> Items { get; set; } = [];
}