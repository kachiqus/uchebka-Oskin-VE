using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace oskin_site.Models;

[Table("users")]
[Index("Username", Name = "users_username_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("users_id")]
    public int UsersId { get; set; }

    [Column("username")]
    [StringLength(64)]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Column("role")]
    [StringLength(32)]
    public string Role { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
