using System.ComponentModel.DataAnnotations;

namespace oskin_site.Dtos;

public sealed class ProductUpdateDto
{
    [Range(1, long.MaxValue, ErrorMessage = "Артикул должен быть положительным целым числом.")]
    public long Article { get; set; }

    [StringLength(300, MinimumLength = 2, ErrorMessage = "Наименование должно содержать от 2 до 300 символов.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Тип продукции указан некорректно.")]
    public int ProductTypeId { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Минимальная стоимость не может быть отрицательной.")]
    public decimal MinPartnerPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Основной материал указан некорректно.")]
    public int MainMaterialTypeId { get; set; }
}
