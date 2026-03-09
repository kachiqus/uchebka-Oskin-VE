using System.ComponentModel.DataAnnotations;

namespace oskin_site.Dtos;

public sealed class ProductWorkshopUpdateDto
{
    [Range(typeof(decimal), "0", "999999", ErrorMessage = "Время изготовления не может быть отрицательным.")]
    public decimal ManufactureHours { get; set; }
}
