using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace oskin_site.Models;

[Table("material_type")]
[Index("Name", Name = "material_type_name_key", IsUnique = true)]
public partial class MaterialType
{
    [Key]
    [Column("material_type_id")]
    public int MaterialTypeId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("loss_percent")]
    [Precision(10, 4)]
    public decimal LossPercent { get; set; }

    [InverseProperty("MainMaterialType")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
