using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;
using SAG2.Classes;
using System.Data.SqlClient;

namespace SAG2.Controllers
{
    public class ImprimirController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Constantes ctes = new Constantes();

        //
        // GET: /Imprimir/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Ingreso(int id)
        {
            Movimiento movimiento = db.Movimiento.Find(id);
            Persona Persona = (Persona)Session["Persona"];
            @ViewBag.Ejecutor = Persona.NombreCompleto;

            try
            {
                @ViewBag.Director = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 1).Where(r => r.ProyectoID == movimiento.ProyectoID).Single().Persona.NombreCompleto;
                @ViewBag.Apoderado = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 6).Where(r => r.ProyectoID == movimiento.ProyectoID).Single().Persona.NombreCompleto;  
            }
            catch
            {}

            return View(movimiento);
        }

        public ActionResult Reintegro(int id)
        {
            Movimiento movimiento = db.Movimiento.Find(id);
            Persona Persona = (Persona)Session["Persona"];
            @ViewBag.Ejecutor = Persona.NombreCompleto;
            DetalleEgreso detalle = db.DetalleEgreso.Find(movimiento.DetalleEgresoID);
            @ViewBag.detalle = detalle;

            try
            {
                @ViewBag.Director = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 1).Where(r => r.ProyectoID == movimiento.ProyectoID).Single().Persona.NombreCompleto;
                @ViewBag.Apoderado = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 6).Where(r => r.ProyectoID == movimiento.ProyectoID).Single().Persona.NombreCompleto;
            }
            catch
            { }

            return View(movimiento);
        }

        public ActionResult Egreso(int id)
        {
            Movimiento movimiento = db.Movimiento.Find(id);
            Persona Persona = (Persona)Session["Persona"];
            @ViewBag.Ejecutor = Persona.NombreCompleto;
            @ViewBag.Detalles = db.DetalleEgreso.Where(d => d.MovimientoID == movimiento.ID).ToList();

            try
            {
                @ViewBag.Director = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 1).Where(r => r.ProyectoID == movimiento.ProyectoID).Single().Persona.NombreCompleto;
                @ViewBag.Apoderado = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 6).Where(r => r.ProyectoID == movimiento.ProyectoID).Single().Persona.NombreCompleto;
            }
            catch
            { }

            return View(movimiento);
        }

        public ActionResult DeudaPendiente(int id)
        {
            DeudaPendiente dp = db.DeudaPendiente.Find(id);
            Persona Persona = (Persona)Session["Persona"];
            @ViewBag.Ejecutor = Persona.NombreCompleto;

            try
            {
                @ViewBag.Director = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 1).Where(r => r.ProyectoID == dp.ProyectoID).Single().Persona.NombreCompleto;
                @ViewBag.Apoderado = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 6).Where(r => r.ProyectoID == dp.ProyectoID).Single().Persona.NombreCompleto;
            }
            catch
            { }

            return View(dp);
        }

        [HttpGet]
        public ActionResult InformeIngreso(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Desde = Desde;
            @ViewBag.Hasta = Hasta;

            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                var ingresos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }

            return null;
        }

        [HttpGet]
        public ActionResult InformeEgreso(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Desde = Desde;
            @ViewBag.Hasta = Hasta;

            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                //var movimientos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                var movimientos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(movimientos.ToList());
                //var egresos = db.DetalleEgreso.Where(m => m.Egreso.Fecha >= Inicio).Where(m => m.Egreso.Fecha <= Fin).Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderBy(m => m.FechaEmision);
                //return View(egresos.ToList());
            }

            return null;
        }

        [HttpGet]
        public ActionResult InformeReintegro(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Desde = Desde;
            @ViewBag.Hasta = Hasta;

            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                var reintegros = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(reintegros.ToList());
            }

            return null;
        }

        [HttpGet]
        public ActionResult InformeFondoFijo(int Mes = 0, int Periodo = 0, int Grupo = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Mes = Mes;
            @ViewBag.Periodo = Periodo;
            if (Grupo > 0)
            {
                var fondofijo = db.FondoFijo.Include(f => f.Cuenta).Include(f => f.Proyecto).Where(f => f.ProyectoID == Proyecto.ID && f.FondoFijoGrupoID == Grupo).OrderBy(f => f.Cuenta.Orden);
                return View(fondofijo.ToList());
            }
            else
            {
                if (Mes > 0 && Periodo > 0)
                {
                    var fondofijo = db.FondoFijo.Include(f => f.Cuenta).Include(f => f.Proyecto).Where(f => f.ProyectoID == Proyecto.ID).Where(f => f.Mes == Mes).Where(f => f.Periodo == Periodo).OrderBy(f => f.Cuenta.Orden);
                    return View(fondofijo.ToList());
                }
            }
            
            return null;
        }

        [HttpGet]
        public ActionResult InformeHonorarios(int Mes = 0, int Periodo = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Mes = Mes;
            @ViewBag.Periodo = Periodo;

            if (Mes > 0 && Periodo > 0)
            {
                var boletahonorario = db.BoletaHonorario.Include(b => b.Persona).Include(b => b.Proyecto).Where(f => f.ProyectoID == Proyecto.ID).Where(b => b.Mes == Mes).Where(b => b.Periodo == Periodo);
                return View(boletahonorario.ToList());
            }

            return null;
        }

        [HttpGet]
        public ActionResult InformeDeuda(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Desde = Desde;
            @ViewBag.Hasta = Hasta;

            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                var deudapendientes = db.DeudaPendiente.Include(d => d.Persona).Include(d => d.Proveedor).Include(d => d.Documento).Include(d => d.Cuenta).Include(d => d.Proyecto).Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID && m.CuentaID != 1).OrderBy(d => d.Cuenta.Orden);
                return View(deudapendientes.ToList());
            }

            return null;
        }

        [HttpGet]
        public ActionResult LibroBanco(int Periodo, int Mes)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];

            try
            {
                ViewBag.SaldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == Mes).Single().SaldoInicialCartola;
            }
            catch (Exception)
            {
                ViewBag.SaldoInicial = 0;
            }

            var movimientos = db.Movimiento.Where(m => m.Periodo == Periodo).Where(m => m.Mes == Mes).Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && a.Eliminado == null && ((a.CuentaID != 1 && a.CuentaID != 6) || a.CuentaID == null)).OrderBy(m => m.Fecha).ThenBy(m => m.NumeroComprobante);
            ViewBag.periodo = Periodo;
            ViewBag.mes = Mes;
            ViewBag.cuentaCorriente = CuentaCorriente;
            ViewBag.NumeroCuenta = CuentaCorriente.Numero;
            return View(movimientos.ToList());
        }

        [HttpGet]
        public ActionResult ExcelLibroBanco(int Periodo, int Mes)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];

            try
            {
                ViewBag.SaldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == Mes).Single().SaldoInicialCartola;
            }
            catch (Exception)
            {
                ViewBag.SaldoInicial = 0;
            }

            var movimientos = db.Movimiento.Where(m => m.Periodo == Periodo).Where(m => m.Mes == Mes).Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && a.Eliminado == null && ((a.CuentaID != 1 && a.CuentaID != 6) || a.CuentaID == null)).OrderBy(m => m.Fecha).ThenBy(m => m.NumeroComprobante);
            ViewBag.periodo = Periodo;
            ViewBag.mes = Mes;
            ViewBag.cuentaCorriente = CuentaCorriente;
            ViewBag.NumeroCuenta = CuentaCorriente.Numero;
            return View(movimientos.ToList());
        }

        public ActionResult Conciliacion(int Periodo, int Mes)
        {
            ViewBag.SaldoCartola = "0";
            ViewBag.GastosBancarios = "0";
            ViewBag.Depositos = "0";
            int periodo = Periodo;
            int mes = Mes;
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];

            try
            {
                ViewBag.SaldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == Mes).Single().SaldoInicialCartola;
            }
            catch (Exception)
            {
                ViewBag.SaldoInicial = 0;
            }

            var movimientos = from m in db.Movimiento
                              where (m.ProyectoID == Proyecto.ID) && ((m.Periodo == periodo && m.Mes == mes) || (m.Periodo < periodo) || (m.Periodo == periodo && m.Mes < mes)) && m.Temporal == null && m.Nulo == null && m.Eliminado == null && ((m.CuentaID != 1 && m.CuentaID != 6) || m.CuentaID == null)
                              orderby m.Periodo, m.Fecha, m.NumeroComprobante
                              select m;

            ViewBag.periodo = Periodo;
            ViewBag.mes = Mes;
            ViewBag.cuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            ViewBag.NumeroCuenta = @ViewBag.cuentaCorriente.Numero;
            //ViewBag.Cheques = movimientos.Sum(m => m.Monto_Egresos);

            try
            {
                var detalles = from m in db.Movimiento
                               where (m.ProyectoID == Proyecto.ID) && ((m.Periodo == Periodo && m.Mes == Mes) || (m.Periodo < Periodo) || (m.Periodo == Periodo && m.Mes < Mes)) && m.Temporal == null && m.Eliminado == null && m.Nulo == null && ((m.CuentaID != 1 && m.CuentaID != 6) || m.CuentaID == null)
                                  select m;
                ViewBag.Cheques = detalles.Sum(d => d.Monto_Egresos);
                //ViewBag.Cheques = db.DetalleEgreso.Where(d => d.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo <= Periodo).Where(m => m.Egreso.Mes <= Mes).Where(d => d.Conciliado == null && d.Temporal == null && d.Nulo == null && d.CuentaID != 1 && d.CuentaID != 6).Sum(d => d.Monto);
            }
            catch (Exception)
            {
                ViewBag.Cheques = 0;
            }

            try
            {
                Conciliacion Conciliacion = db.Conciliacion.Where(c => c.ProyectoID == Proyecto.ID).Where(c => c.Periodo == Periodo).Where(c => c.Mes == Mes).Single();
                ViewBag.ConciliacionID = Conciliacion.ID;
                ViewBag.SaldoCartola = Conciliacion.SaldoCartola;
                ViewBag.GastosBancarios = Conciliacion.Gastos;
                ViewBag.Depositos = Conciliacion.Depositos;
                ViewBag.Fecha = Conciliacion.FechaCartola.ToShortDateString();
                ViewBag.SaldoLibro = Conciliacion.SaldoCartola + Conciliacion.Depositos - ViewBag.Cheques + Conciliacion.Gastos;
            }
            catch (Exception)
            {
                ViewBag.SaldoCartola = 0;
                ViewBag.GastosBancarios = 0;
                ViewBag.Depositos = 0;
                ViewBag.Fecha = DateTime.Now.ToShortDateString();
                ViewBag.SaldoLibro = 0;
            }

            return View(movimientos.ToList());
        }
    }
}
