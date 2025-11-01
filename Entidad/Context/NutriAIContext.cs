using System;
using System.Collections.Generic;
using Entidad.Models;
using Microsoft.EntityFrameworkCore;

namespace Entidad.Context;

public partial class NutriAIContext : DbContext
{
    public NutriAIContext()
    {
    }

    public NutriAIContext(DbContextOptions<NutriAIContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC077EEAE9A8");

            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages).HasConstraintName("FK_ChatMessage_Session");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatSess__3214EC07CFBDE0F4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MessageCount).HasDefaultValue(0);

            entity.HasOne(d => d.Usuario).WithMany(p => p.ChatSessions).HasConstraintName("FK_ChatSession_Usuario");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserInfo__3214EC07EB1F814C");

            entity.HasOne(d => d.Usuario).WithOne(p => p.UserInfo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserInfo_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC0799336185");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
