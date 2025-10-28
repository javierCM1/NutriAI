using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entidad.Models;

[Table("ChatMessage")]
public partial class ChatMessage
{
    [Key]
    public int Id { get; set; }

    public int SessionId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsUserMessage { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [ForeignKey("SessionId")]
    [InverseProperty("ChatMessages")]
    public virtual ChatSession Session { get; set; } = null!;
}
