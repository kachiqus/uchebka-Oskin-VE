using System.ComponentModel.DataAnnotations;

namespace oskin_site.Dtos;

public sealed class ProductWorkshopCreateDto
{
    [Required(ErrorMessage = "Укажите продукцию.")]
    [Range(1, long.MaxValue, ErrorMessage = "Идентификатор продукции указан некорректно.")]
    public long ProductId { get; set; }

    [Required(ErrorMessage = "Укажите цех.")]
    [Range(1, int.MaxValue, ErrorMessage = "Идентификатор цеха указан некорректно.")]
    public int WorkshopId { get; set; }

    [Required(ErrorMessage = "Укажите время изготовления в цехе.")]
    [Range(typeof(decimal), "0", "999999", ErrorMessage = "Время изготовления не может быть отрицательным.")]
    public decimal ManufactureHours { get; set; }
}
