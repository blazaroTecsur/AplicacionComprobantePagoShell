using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Resguardo.Infrastructure.Scaffold;

public partial class ErpTecsurPinternaContext : DbContext
{
    public ErpTecsurPinternaContext()
    {
    }

    public ErpTecsurPinternaContext(DbContextOptions<ErpTecsurPinternaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Rpoaprobador> Rpoaprobadors { get; set; }

    public virtual DbSet<Rpoconfig> Rpoconfigs { get; set; }

    public virtual DbSet<Rpoefectivo> Rpoefectivos { get; set; }

    public virtual DbSet<Rpogenerico> Rpogenericos { get; set; }

    public virtual DbSet<Rpopersonal> Rpopersonals { get; set; }

    public virtual DbSet<Rposervicio> Rposervicios { get; set; }

    public virtual DbSet<Rposervicioprov> Rposervicioprovs { get; set; }

    public virtual DbSet<Rposolicitud> Rposolicituds { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=10.160.9.18;uid=tecsur_root;pwd=Tecsur2021$;database=erp_tecsur_pinterna", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.4-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Rpoaprobador>(entity =>
        {
            entity.HasKey(e => e.IdAprobador).HasName("PRIMARY");

            entity.ToTable("rpoaprobador");

            entity.HasIndex(e => e.IdAprobador, "IdAprobador").IsUnique();

            entity.Property(e => e.IdAprobador).HasColumnType("int(11)");
            entity.Property(e => e.CodSocio).HasMaxLength(15);
            entity.Property(e => e.CodUnidad).HasMaxLength(15);
            entity.Property(e => e.DscUnidad).HasMaxLength(50);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.NomSocio).HasMaxLength(50);
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);
        });

        modelBuilder.Entity<Rpoconfig>(entity =>
        {
            entity.HasKey(e => e.IdConfig).HasName("PRIMARY");

            entity.ToTable("rpoconfig");

            entity.HasIndex(e => e.IdConfig, "IdConfig").IsUnique();

            entity.HasIndex(e => e.IdTpoServicio, "IdTpoServicio");

            entity.Property(e => e.IdConfig).HasColumnType("int(11)");
            entity.Property(e => e.Cantidad).HasColumnType("int(11)");
            entity.Property(e => e.CodActividad).HasMaxLength(15);
            entity.Property(e => e.DscActividad).HasMaxLength(50);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.IdTpoServicio).HasColumnType("int(11)");
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);

            entity.HasOne(d => d.IdTpoServicioNavigation).WithMany(p => p.Rpoconfigs)
                .HasForeignKey(d => d.IdTpoServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rpoconfig_ibfk_1");
        });

        modelBuilder.Entity<Rpoefectivo>(entity =>
        {
            entity.HasKey(e => e.IdEfectivo).HasName("PRIMARY");

            entity.ToTable("rpoefectivo");

            entity.HasIndex(e => e.IdEfectivo, "IdEfectivo").IsUnique();

            entity.HasIndex(e => e.IdPersonal, "IdPersonal");

            entity.HasIndex(e => e.IdServicioProv, "IdServicioProv");

            entity.Property(e => e.IdEfectivo).HasColumnType("int(11)");
            entity.Property(e => e.Comentario).HasMaxLength(100);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.HraFinal)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.HraInicio)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.IdPersonal).HasColumnType("int(11)");
            entity.Property(e => e.IdServicioProv).HasColumnType("int(11)");
            entity.Property(e => e.Telefono).HasMaxLength(15);
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.Rpoefectivos)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rpoefectivo_ibfk_2");

            entity.HasOne(d => d.IdServicioProvNavigation).WithMany(p => p.Rpoefectivos)
                .HasForeignKey(d => d.IdServicioProv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rpoefectivo_ibfk_1");
        });

        modelBuilder.Entity<Rpogenerico>(entity =>
        {
            entity.HasKey(e => e.IdGenerico).HasName("PRIMARY");

            entity.ToTable("rpogenerico");

            entity.HasIndex(e => e.IdGenerico, "IdGenerico").IsUnique();

            entity.Property(e => e.IdGenerico).HasColumnType("int(11)");
            entity.Property(e => e.Codigo).HasMaxLength(15);
            entity.Property(e => e.Descripcion).HasMaxLength(50);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.Tipo).HasMaxLength(10);
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);
        });

        modelBuilder.Entity<Rpopersonal>(entity =>
        {
            entity.HasKey(e => e.IdPersonal).HasName("PRIMARY");

            entity.ToTable("rpopersonal");

            entity.HasIndex(e => e.IdPersonal, "IdPersonal").IsUnique();

            entity.Property(e => e.IdPersonal).HasColumnType("int(11)");
            entity.Property(e => e.Apellidos).HasMaxLength(50);
            entity.Property(e => e.Dni).HasMaxLength(15);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.Nombres).HasMaxLength(50);
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);
        });

        modelBuilder.Entity<Rposervicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PRIMARY");

            entity.ToTable("rposervicio");

            entity.HasIndex(e => e.IdServicio, "IdServicio").IsUnique();

            entity.HasIndex(e => e.IdSolicitud, "IdSolicitud");

            entity.HasIndex(e => e.IdTpoServicio, "IdTpoServicio");

            entity.Property(e => e.IdServicio).HasColumnType("int(11)");
            entity.Property(e => e.Cantidad).HasColumnType("int(11)");
            entity.Property(e => e.CantidadBck).HasColumnType("int(11)");
            entity.Property(e => e.Comentario).HasMaxLength(100);
            entity.Property(e => e.Coordenada).HasMaxLength(50);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.HraAmplia)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.HraFinal)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.HraInicio)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.IdSolicitud).HasColumnType("int(11)");
            entity.Property(e => e.IdTpoServicio).HasColumnType("int(11)");
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.Rposervicios)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposervicio_ibfk_1");

            entity.HasOne(d => d.IdTpoServicioNavigation).WithMany(p => p.Rposervicios)
                .HasForeignKey(d => d.IdTpoServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposervicio_ibfk_2");
        });

        modelBuilder.Entity<Rposervicioprov>(entity =>
        {
            entity.HasKey(e => e.IdServicioProv).HasName("PRIMARY");

            entity.ToTable("rposervicioprov");

            entity.HasIndex(e => e.IdProveedor, "IdProveedor");

            entity.HasIndex(e => e.IdServicio, "IdServicio");

            entity.HasIndex(e => e.IdServicioProv, "IdServicioProv").IsUnique();

            entity.Property(e => e.IdServicioProv).HasColumnType("int(11)");
            entity.Property(e => e.Cantidad).HasColumnType("int(11)");
            entity.Property(e => e.Estado)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.IdProveedor).HasColumnType("int(11)");
            entity.Property(e => e.IdServicio).HasColumnType("int(11)");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Rposervicioprovs)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposervicioprov_ibfk_2");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.Rposervicioprovs)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposervicioprov_ibfk_1");
        });

        modelBuilder.Entity<Rposolicitud>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PRIMARY");

            entity.ToTable("rposolicitud");

            entity.HasIndex(e => e.IdEstado, "IdEstado");

            entity.HasIndex(e => e.IdFlujo, "IdFlujo");

            entity.HasIndex(e => e.IdSolicitud, "IdSolicitud").IsUnique();

            entity.HasIndex(e => e.IdTipo, "IdTipo");

            entity.Property(e => e.IdSolicitud).HasColumnType("int(11)");
            entity.Property(e => e.Celular).HasMaxLength(15);
            entity.Property(e => e.CodActv).HasMaxLength(15);
            entity.Property(e => e.CodCapataz).HasMaxLength(15);
            entity.Property(e => e.CodDpto).HasMaxLength(15);
            entity.Property(e => e.CodSupr).HasMaxLength(15);
            entity.Property(e => e.Comentario).HasMaxLength(200);
            entity.Property(e => e.Coordenada).HasMaxLength(30);
            entity.Property(e => e.Direccion).HasMaxLength(100);
            entity.Property(e => e.FechaAct).HasColumnType("datetime");
            entity.Property(e => e.FechaApro).HasColumnType("datetime");
            entity.Property(e => e.FechaFoc).HasColumnType("datetime");
            entity.Property(e => e.FechaReg).HasColumnType("datetime");
            entity.Property(e => e.Folio).HasMaxLength(9);
            entity.Property(e => e.FolioRef).HasMaxLength(10);
            entity.Property(e => e.IdEstado).HasColumnType("int(11)");
            entity.Property(e => e.IdFlujo).HasColumnType("int(11)");
            entity.Property(e => e.IdTipo).HasColumnType("int(11)");
            entity.Property(e => e.NomActv).HasMaxLength(50);
            entity.Property(e => e.NomCapataz).HasMaxLength(50);
            entity.Property(e => e.NomDpto).HasMaxLength(50);
            entity.Property(e => e.NomSctta).HasMaxLength(50);
            entity.Property(e => e.NomSupr).HasMaxLength(50);
            entity.Property(e => e.NumSro).HasMaxLength(10);
            entity.Property(e => e.RucSctta).HasMaxLength(15);
            entity.Property(e => e.TpoTrabajo).HasMaxLength(200);
            entity.Property(e => e.UsuarioAct).HasMaxLength(50);
            entity.Property(e => e.UsuarioApro).HasMaxLength(50);
            entity.Property(e => e.UsuarioReg).HasMaxLength(50);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.RposolicitudIdEstadoNavigations)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposolicitud_ibfk_2");

            entity.HasOne(d => d.IdFlujoNavigation).WithMany(p => p.RposolicitudIdFlujoNavigations)
                .HasForeignKey(d => d.IdFlujo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposolicitud_ibfk_3");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.RposolicitudIdTipoNavigations)
                .HasForeignKey(d => d.IdTipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rposolicitud_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
