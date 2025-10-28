using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entidad.Models;

[Table("ChatSession")]
public partial class ChatSession
{
    [Key]
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    [StringLength(255)]
    public string? Title { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastMessageTime { get; set; }

    public int? MessageCount { get; set; }

    [InverseProperty("Session")]
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("ChatSessions")]
    public virtual Usuario Usuario { get; set; } = null!;
}
