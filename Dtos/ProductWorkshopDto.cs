namespace oskin_site.Dtos;

public sealed class ProductWorkshopDto
{
    public long ProductId { get; set; }
    public int WorkshopId { get; set; }
    public string WorkshopName { get; set; } = string.Empty;
    public int PeopleCount { get; set; }
    public decimal ManufactureHours { get; set; }
}
