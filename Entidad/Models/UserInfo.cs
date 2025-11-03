using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entidad.Models;

[Table("UserInfo")]
[Index("UsuarioId", Name = "UQ__UserInfo__2B3DE7B97DAC18A3", IsUnique = true)]
public partial class UserInfo
{
    [Key]
    public int Id { get; set; }

    public int? Edad { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Peso { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Altura { get; set; }

    [StringLength(255)]
    public string? PreferenciaAlimenticia { get; set; }

    public int? UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("UserInfo")]
    public virtual Usuario? Usuario { get; set; }
}
