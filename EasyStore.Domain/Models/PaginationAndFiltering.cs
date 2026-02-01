namespace EasyStore.Domain.Models;

public class PaginationAndFiltering
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }
    public string? FilterQuery { get; set; }
}

public class Paginated<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
