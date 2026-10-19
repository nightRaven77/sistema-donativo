using apiDonativos.Data.Interceptors;
using apiDonativos.Models;
using apiDonativos.Models.Catalogos;
using apiDonativos.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace apiDonativos
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        #region Catalogos
        public DbSet<Donador> Donadores { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }
        public DbSet<contactoDonador> contactos_donadores { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<Beneficiario> Beneficiarios { get; set; }
        #endregion

        #region Registros
        public DbSet<Donativo> donativos { get; set; }
        public DbSet<DonativoDetalle> donativos_detalle { get; set; }
        public DbSet<DonativoArchivo> donativos_archivos { get; set; }
        public DbSet<Inventario> inventario { get; set; }
        public DbSet<Salida> salidas { get; set; }
        public DbSet<SalidaDetalle> salidas_detalle { get; set; }
        public DbSet<SalidasArchivo> salidas_archivos { get; set; }
        public DbSet<Traspaso> Traspasos { get; set; }
        public DbSet<TraspasoDetalle> traspasos_detalle { get; set; }
        public DbSet<TraspasoArchivo> traspasos_archivos { get; set; }
        public DbSet<MovimientoInventario> movimientos_inventario { get; set; }

        #endregion
        #region Usuarios
        public DbSet<RegistrarUsuarioResultado> RegistrarUsuarioResultados { get; set; }
        //public DbSet<ActualizarPassResultado> ActualizarPassResultados{ get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Registrar el interceptor de auditoría
            optionsBuilder.AddInterceptors(new AuditInterceptor());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar claves primarias
            modelBuilder.Entity<Donador>()
                .HasKey(d => d.id_donador);

            modelBuilder.Entity<Bitacora>()
                .HasKey(b => b.id_bitacora);

            modelBuilder.Entity<contactoDonador>()
                .HasKey(c => c.id_contacto);

            modelBuilder.Entity<Articulo>()
                .HasKey(a => a.id_articulo);

            modelBuilder.Entity<Categoria>()
                .HasKey(c => c.id_categoria);

            modelBuilder.Entity<Beneficiario>()
                .HasKey(b => b.id_beneficiario);

            modelBuilder.Entity<Donativo>()
                .HasKey(d => d.id_donativo);

            modelBuilder.Entity<DonativoDetalle>()
                .HasKey(dd => dd.id_donativo_detalle);

            modelBuilder.Entity<DonativoArchivo>()
                .HasKey(da => da.id_archivo);

            modelBuilder.Entity<Salida>()
                .HasKey(s => s.id_salida);  // ✅ CORREGIDO: era id_Salida

            modelBuilder.Entity<SalidaDetalle>()
                .HasKey(sd => sd.id_detalle);

            modelBuilder.Entity<SalidasArchivo>()
                .HasKey(sa => sa.id_archivo);

            modelBuilder.Entity<Almacen>()
                .HasKey(a => a.id_almacen);

            modelBuilder.Entity<Inventario>()
                .HasKey(i => i.id_inventario);

            modelBuilder.Entity<Traspaso>()
                .HasKey(t => t.id_traspaso);

            modelBuilder.Entity<TraspasoDetalle>()
                .HasKey(td => td.id_detalle);

            modelBuilder.Entity<TraspasoArchivo>()
                .HasKey(ta => ta.id_archivo);

            modelBuilder.Entity<MovimientoInventario>()
                .HasKey(m => m.id_movimiento);

            modelBuilder.Entity<RegistrarUsuarioResultado>()
                .HasNoKey();

            // Configurar relación de clave foránea
            modelBuilder.Entity<Articulo>()
                .HasOne(a => a.categoria)
                .WithMany()
                .HasForeignKey(a => a.id_categoria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonativoDetalle>()
                .HasOne(d => d.donativo)
                .WithMany()
                .HasForeignKey(d => d.id_donativo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonativoArchivo>()
                .HasOne(da => da.donativo)
                .WithMany()
                .HasForeignKey(da => da.id_donativo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalidaDetalle>()
                .HasOne(sd => sd.salida)
                .WithMany()
                .HasForeignKey(sd => sd.id_salida)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalidasArchivo>()
               .HasOne(sa => sa.salida)  // ✅ CORREGIDO: era "donativo"
               .WithMany()
               .HasForeignKey(sa => sa.id_salida)
               .OnDelete(DeleteBehavior.Restrict);

            // ✅ NUEVO: Configurar relación para TraspasoArchivo
            modelBuilder.Entity<TraspasoArchivo>()
                .HasOne(ta => ta.Traspaso)
                .WithMany()
                .HasForeignKey(ta => ta.id_traspaso)
                .OnDelete(DeleteBehavior.Restrict);

            // Filtros globales para excluir registros inactivos
            modelBuilder.Entity<contactoDonador>()
                .HasQueryFilter(c => c.activo);

            modelBuilder.Entity<Donador>()
                .HasQueryFilter(d => d.activo);

            modelBuilder.Entity<Articulo>()
                .HasQueryFilter(a => a.activo);

            modelBuilder.Entity<DonativoArchivo>()
                .HasQueryFilter(da => da.activo);

            modelBuilder.Entity<SalidasArchivo>()
                .HasQueryFilter(sa => sa.activo);

            modelBuilder.Entity<Donativo>()
                .HasQueryFilter(d => d.estatus.ToLower() != "cancelado");

            modelBuilder.Entity<Salida>()
                .HasQueryFilter(s => s.estatus.ToLower() != "cancelada");

            modelBuilder.Entity<Traspaso>()
                .HasQueryFilter(t => t.estatus.ToLower() != "cancelado");

            modelBuilder.Entity<TraspasoArchivo>()
                .HasQueryFilter(ta => ta.activo == true);

            modelBuilder.Entity<Bitacora>()
                .Property(b => b.fecha_hora)
                .HasDefaultValueSql("GETDATE()");

            // ✅ NUEVO: Valores por defecto para fechas en Salida
            modelBuilder.Entity<Salida>()
                .Property(s => s.fecha_registro)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Salida>()
                .Property(s => s.fecha_actualizacion)
                .HasDefaultValueSql("GETDATE()");

        }
    }
}