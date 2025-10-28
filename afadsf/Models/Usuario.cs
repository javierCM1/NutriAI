using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entidad.Models;

[Table("Usuario")]
[Index("Email", Name = "UQ__Usuario__A9D10534D14DADB9", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [InverseProperty("Usuario")]
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    [InverseProperty("Usuario")]
    public virtual UserInfo? UserInfo { get; set; }
}
