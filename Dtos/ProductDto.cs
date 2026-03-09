namespace oskin_site.Dtos;

public sealed class ProductDto
{
    public long ProductId { get; set; }
    public long Article { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public decimal MinPartnerPrice { get; set; }
    public int MainMaterialTypeId { get; set; }
    public string MainMaterialTypeName { get; set; } = string.Empty;
    public int ManufactureTimeHours { get; set; }
}
