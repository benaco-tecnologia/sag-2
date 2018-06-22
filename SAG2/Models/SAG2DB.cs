using System.Data.Entity;

namespace SAG2.Models
{
    public class SAG2DB : DbContext
    {
        public DbSet<Banco> Banco { get; set; }
        public DbSet<CuentaCorriente> CuentaCorriente { get; set; }
        public DbSet<Direccion> Direccion { get; set; }
        public DbSet<Proyecto> Proyecto { get; set; }
        public DbSet<Movimiento> Movimiento { get; set; }
        public DbSet<Persona> Persona { get; set; }
        public DbSet<SistemaAsistencial> SistemaAsistencial { get; set; }
        public DbSet<Proveedor> Proveedor { get; set; }

        // Agregados el 24 de Enero de 2012
        public DbSet<FondoFijo> FondoFijo { get; set; }
        public DbSet<FondoFijoGrupo> FondoFijoGrupo { get; set; }
        public DbSet<ItemGasto> ItemGasto { get; set; }
        public DbSet<Usuario> Usuario { get; set; }

        // 25 de Enero 2012
        public DbSet<DepositoPlazo> DepositoPlazo { get; set; }
        public DbSet<Region> Region { get; set; }
        public DbSet<Comuna> Comuna { get; set; }
        public DbSet<Pais> Pais { get; set; }
        public DbSet<LineasAtencion> LineasAtencion { get; set; }
        public DbSet<Documento> Documento { get; set; }

        // 12 Feb 2012
        public DbSet<Saldo> Saldo { get; set; }
        public DbSet<Rol> Rol { get; set; }

        public DbSet<ObjetivoCuenta> ObjetivoCuenta { get; set; }
        public DbSet<ItemIntervencion> ItemIntervencion { get; set; }
        public DbSet<Profesion> Profesion { get; set; }
        public DbSet<Especializacion> Especializacion { get; set; }
        public DbSet<Cargo> Cargo { get; set; }
        public DbSet<Servicio> Servicio { get; set; }
        public DbSet<Contrato> Contrato { get; set; }

        // Tipos
        public DbSet<TipoAsistenciaPersonal> TipoAsistenciaPersonal { get; set; }
        public DbSet<TipoBajaInventario> TipoBajaInventario { get; set; }
        public DbSet<TipoComprobante> TipoComprobante { get; set; }
        public DbSet<TipoImputacion> TipoImputacion { get; set; }
        public DbSet<TipoOrigenAdquisicion> TipoOrigenAdquisicion { get; set; }
        public DbSet<TipoPersonal> TipoPersonal { get; set; }
        public DbSet<TipoProyecto> TipoProyecto { get; set; }
        public DbSet<TipoRol> TipoRol { get; set; }
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<Convenio> Convenio { get; set; }
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<Supervision> Supervision { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<Seccion> Seccion { get; set; }
        public DbSet<Cuenta> Cuenta { get; set; }
        public DbSet<BoletaHonorario> BoletaHonorario { get; set; }
        public DbSet<DeudaPendiente> DeudaPendiente { get; set; }
        public DbSet<DetalleEgreso> DetalleEgreso { get; set; }
        public DbSet<Intervencion> Intervencion { get; set; }

        // Mantenedores > Inventarios
        public DbSet<Articulo> Articulo { get; set; }
        public DbSet<UnidadMedida> UnidadMedida { get; set; }
        public DbSet<Bodega> Bodega { get; set; }
        public DbSet<MovimientosBodega> MovimientoBodega { get; set; }
        public DbSet<RolProveedor> RolProveedor { get; set; }
        public DbSet<InventarioAltas> InventarioAltas { get; set; }

        public DbSet<Presupuesto> Presupuesto { get; set; }
        public DbSet<DetallePresupuesto> DetallePresupuesto { get; set; }

        // Conciliacion
        public DbSet<Conciliacion> Conciliacion { get; set; }
        public DbSet<ConciliacionRegistro> ConciliacionRegistro { get; set; }


        // Autorizaciones
        public DbSet<OpcionesSupervision> OpcionesSupervision { get; set; }
        public DbSet<Autorizacion> Autorizacion { get; set; }


        // Auditorias
        public DbSet<ProgramaAnualAuditorias> ProgramaAnualAuditorias { get; set; }
        public DbSet<AuditoriasDocumento> AuditoriasDocumento { get; set; }
        public DbSet<AuditoriasMetodologia> AuditoriasMetodologia { get; set; }
        public DbSet<AuditoriasObjetivo> AuditoriasObjetivo { get; set; }
        public DbSet<PlanTrabajoAuditoria> PlanTrabajoAuditoria { get; set; }
        public DbSet<InformeAuditoria> InformeAuditoria { get; set; }
        public DbSet<TipoAuditoria> TipoAuditoria { get; set; }

        // Indice Gestion
        public DbSet<IndicadoresGestion> IndicadoresGestion { get; set; }

        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<Especie> Especie { get; set; }
        public DbSet<Dependencia> Dependencia { get; set; }
        public DbSet<InventarioBajas> InventarioBajas { get; set; }

        public DbSet<RegistroModificacionProyecto> RegistroModificacionProyecto { get; set; }


        // Inventario 1.1
        public DbSet<InventarioAlta> InventarioAlta { get; set; }
        public DbSet<InventarioBaja> InventarioBaja { get; set; }
        public DbSet<InventarioHoja> InventarioHoja { get; set; }
        public DbSet<InventarioItem> InventarioItem { get; set; }
        public DbSet<InventarioMovimiento> InventarioMovimiento { get; set; }
        public DbSet<InventarioTraslado> InventarioTraslado { get; set; }
        public DbSet<AutorizacionMovimiento> AutorizacionMovimiento { get; set; }

        // Preguntas Frecuestes
        public DbSet<PreguntaFrecuente> PreguntaFrecuente { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Banco>().ToTable("Banco");
            modelBuilder.Entity<CuentaCorriente>().ToTable("CuentaCorriente");
            modelBuilder.Entity<Direccion>().ToTable("Direccion");
            modelBuilder.Entity<Proyecto>().ToTable("Proyectos");
            modelBuilder.Entity<Movimiento>().ToTable("Movimiento");
            modelBuilder.Entity<Persona>().ToTable("Persona");
            modelBuilder.Entity<SistemaAsistencial>().ToTable("SistemaAsistencial");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedor");

            // Agregados el 24 de Enero de 2012
            modelBuilder.Entity<ItemGasto>().ToTable("ItemGasto");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");

            // 25 de Enero de 2012
            modelBuilder.Entity<DepositoPlazo>().ToTable("DepositoPlazo");
            modelBuilder.Entity<Region>().ToTable("Regiones");
            modelBuilder.Entity<Pais>().ToTable("Paises");
            modelBuilder.Entity<Comuna>().ToTable("Comunas");
            modelBuilder.Entity<LineasAtencion>().ToTable("LineasAtencion");
            modelBuilder.Entity<Documento>().ToTable("Documento");

            // 12 Feb 2012
            modelBuilder.Entity<Saldo>().ToTable("Saldo");
            modelBuilder.Entity<Rol>().ToTable("Rol");

            modelBuilder.Entity<ObjetivoCuenta>().ToTable("ObjetivoCuenta");
            modelBuilder.Entity<ItemIntervencion>().ToTable("ItemIntervencion");
            modelBuilder.Entity<Profesion>().ToTable("Profesion");
            modelBuilder.Entity<Especializacion>().ToTable("Especializacion");
            modelBuilder.Entity<Cargo>().ToTable("Cargo");

            // Tipos
            modelBuilder.Entity<TipoAsistenciaPersonal>().ToTable("TipoAsistenciaPersonal");
            modelBuilder.Entity<TipoBajaInventario>().ToTable("TipoBajaInventario");
            modelBuilder.Entity<TipoComprobante>().ToTable("TipoComprobante");
            modelBuilder.Entity<TipoImputacion>().ToTable("TipoImputacion");
            modelBuilder.Entity<TipoOrigenAdquisicion>().ToTable("TipoOrigenAdquisicion");
            modelBuilder.Entity<TipoPersonal>().ToTable("TipoPersonal");
            modelBuilder.Entity<TipoRol>().ToTable("TipoRol");
            modelBuilder.Entity<TipoProyecto>().ToTable("TipoProyecto");
            modelBuilder.Entity<Convenio>().ToTable("Convenios");
            modelBuilder.Entity<Periodo>().ToTable("Periodos");

            modelBuilder.Entity<Supervision>().ToTable("Supervisiones");
            modelBuilder.Entity<Auditoria>().ToTable("Auditorias");
            modelBuilder.Entity<Seccion>().ToTable("Secciones");
            modelBuilder.Entity<Permiso>().ToTable("Permisos");

            modelBuilder.Entity<Servicio>().ToTable("Servicios");
            modelBuilder.Entity<Contrato>().ToTable("Contratos");
            modelBuilder.Entity<Cuenta>().ToTable("Cuentas");
            modelBuilder.Entity<DeudaPendiente>().ToTable("DeudaPendiente");
            modelBuilder.Entity<FondoFijo>().ToTable("FondoFijo");
            modelBuilder.Entity<FondoFijoGrupo>().ToTable("FondoFijoGrupo");
            modelBuilder.Entity<BoletaHonorario>().ToTable("BoletaHonorario");
            modelBuilder.Entity<DetalleEgreso>().ToTable("DetalleEgreso");
            modelBuilder.Entity<Intervencion>().ToTable("Intervenciones");

            // Mantenedores > Inventarios
            modelBuilder.Entity<Articulo>().ToTable("Articulos");
            modelBuilder.Entity<UnidadMedida>().ToTable("UnidadesMedida");
            modelBuilder.Entity<Bodega>().ToTable("Bodega");
            modelBuilder.Entity<MovimientosBodega>().ToTable("BodegaMovimientos");

            modelBuilder.Entity<Presupuesto>().ToTable("Presupuesto");
            modelBuilder.Entity<DetallePresupuesto>().ToTable("DetallePresupuesto");
            modelBuilder.Entity<RolProveedor>().ToTable("RolProveedor");

            modelBuilder.Entity<Conciliacion>().ToTable("Conciliacion");
            modelBuilder.Entity<ConciliacionRegistro>().ToTable("ConciliacionRegistro");
            modelBuilder.Entity<OpcionesSupervision>().ToTable("OpcionesSupervision");
            modelBuilder.Entity<Autorizacion>().ToTable("Autorizaciones");
            modelBuilder.Entity<InventarioAltas>().ToTable("InventarioAltas");

            // Auditorias
            modelBuilder.Entity<ProgramaAnualAuditorias>().ToTable("ProgramaAnualAuditorias");
            modelBuilder.Entity<AuditoriasDocumento>().ToTable("AuditoriasDocumento");
            modelBuilder.Entity<AuditoriasMetodologia>().ToTable("AuditoriasMetodologia");
            modelBuilder.Entity<AuditoriasObjetivo>().ToTable("AuditoriasObjetivo");
            modelBuilder.Entity<PlanTrabajoAuditoria>().ToTable("PlanTrabajoAuditoria");

            modelBuilder.Entity<IndicadoresGestion>().ToTable("IndicadoresGestion");
            modelBuilder.Entity<InformeAuditoria>().ToTable("InformeAuditoria");
            modelBuilder.Entity<TipoAuditoria>().ToTable("TipoAuditoria");

            modelBuilder.Entity<Inventario>().ToTable("Inventario");
            modelBuilder.Entity<Especie>().ToTable("Especie");
            modelBuilder.Entity<Dependencia>().ToTable("Dependencia");
            modelBuilder.Entity<InventarioBajas>().ToTable("InventarioBajas");

            modelBuilder.Entity<RegistroModificacionProyecto>().ToTable("RegistroModificacionProyecto");

            // Inventario 1.1
            modelBuilder.Entity<InventarioAlta>().ToTable("InventarioAltaDetalle");
            modelBuilder.Entity<InventarioBaja>().ToTable("InventarioBajaDetalle");
            modelBuilder.Entity<InventarioHoja>().ToTable("InventarioHojaDetalle");
            modelBuilder.Entity<InventarioItem>().ToTable("InventarioItem");
            modelBuilder.Entity<InventarioMovimiento>().ToTable("InventarioMovimiento");
            modelBuilder.Entity<InventarioTraslado>().ToTable("InventarioTrasladoDetalle");

            modelBuilder.Entity<PreguntaFrecuente>().ToTable("PreguntasFrecuentes");
            modelBuilder.Entity<AutorizacionMovimiento>().ToTable("AutorizacionMovimiento");
        }
    }
}