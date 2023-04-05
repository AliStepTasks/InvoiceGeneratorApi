namespace InvoiceGeneratorApi.DTO.Pagination;

public class PaginationDTO<TModel>
{
    public IEnumerable<TModel> Items { get; set; }
    public PaginatinoMeta Meta { get; set;}

    public PaginationDTO(IEnumerable<TModel> items, PaginatinoMeta meta)
    {
        Items = items;
        Meta = meta;
    }
    public PaginationDTO()
    {
        
    }
}