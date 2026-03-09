using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace oskin_site.Models;

[Table("product_type")]
[Index("Name", Name = "product_type_name_key", IsUnique = true)]
public partial class ProductType
{
    [Key]
    [Column("product_type_id")]
    public int ProductTypeId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("coefficient")]
    [Precision(10, 2)]
    public decimal Coefficient { get; set; }

    [InverseProperty("ProductType")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
