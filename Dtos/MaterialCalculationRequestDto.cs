using System.ComponentModel.DataAnnotations;

namespace oskin_site.Dtos;

public sealed class MaterialCalculationRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductTypeId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MaterialTypeId { get; set; }

    [Required]
    [Range(typeof(decimal), "0.0001", "999999999")]
    public decimal ProductCount { get; set; }

    [Required]
    [Range(typeof(decimal), "0.0001", "999999999")]
    public decimal Param1 { get; set; }

    [Required]
    [Range(typeof(decimal), "0.0001", "999999999")]
    public decimal Param2 { get; set; }
}
