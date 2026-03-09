using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace oskin_site.Models;

[PrimaryKey("ProductId", "WorkshopId")]
[Table("product_workshop")]
[Index("WorkshopId", Name = "ix_product_workshop_workshop_id")]
public partial class ProductWorkshop
{
    [Key]
    [Column("product_id")]
    public long ProductId { get; set; }

    [Key]
    [Column("workshop_id")]
    public int WorkshopId { get; set; }

    [Column("manufacture_hours")]
    [Precision(10, 2)]
    public decimal ManufactureHours { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductWorkshops")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("WorkshopId")]
    [InverseProperty("ProductWorkshops")]
    public virtual Workshop Workshop { get; set; } = null!;
}
