namespace InvoiceGeneratorApi.DTO.Pagination;

public class PaginatinoMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    public PaginatinoMeta(int page, int pageSize, int count)
    {
        Page = page;
        PageSize = pageSize;
        TotalPages = Convert.ToInt32(Math.Ceiling(1.0 * count / pageSize));
    }
}