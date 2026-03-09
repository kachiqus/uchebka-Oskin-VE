using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace oskin_site.Models;

[Table("workshop")]
[Index("Name", Name = "workshop_name_key", IsUnique = true)]
public partial class Workshop
{
    [Key]
    [Column("workshop_id")]
    public int WorkshopId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("workshop_type")]
    [StringLength(120)]
    public string WorkshopType { get; set; } = null!;

    [Column("people_count")]
    public int PeopleCount { get; set; }

    [InverseProperty("Workshop")]
    public virtual ICollection<ProductWorkshop> ProductWorkshops { get; set; } = new List<ProductWorkshop>();
}
