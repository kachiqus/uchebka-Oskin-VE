using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace oskin_site.Models;

[Table("product")]
[Index("MainMaterialTypeId", Name = "ix_product_main_material_type_id")]
[Index("ProductTypeId", Name = "ix_product_product_type_id")]
[Index("Article", Name = "product_article_key", IsUnique = true)]
public partial class Product
{
    [Key]
    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("article")]
    public long Article { get; set; }

    [Column("name")]
    [StringLength(300)]
    public string Name { get; set; } = null!;

    [Column("product_type_id")]
    public int ProductTypeId { get; set; }

    [Column("min_partner_price")]
    [Precision(14, 2)]
    public decimal MinPartnerPrice { get; set; }

    [Column("main_material_type_id")]
    public int MainMaterialTypeId { get; set; }

    [ForeignKey("MainMaterialTypeId")]
    [InverseProperty("Products")]
    public virtual MaterialType MainMaterialType { get; set; } = null!;

    [ForeignKey("ProductTypeId")]
    [InverseProperty("Products")]
    public virtual ProductType ProductType { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<ProductWorkshop> ProductWorkshops { get; set; } = new List<ProductWorkshop>();
}
