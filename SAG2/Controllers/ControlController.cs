using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;

namespace SAG2.Controllers
{
    public class ControlController : Controller
    {
        private SAG2DB db = new SAG2DB();
        //
        // GET: /Control/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Intervenciones(int Periodo = 0, string export = "") 
        {
            if (Periodo == 0)
            { 
                Periodo = (int)Session["Periodo"];
            }

            ViewBag.Exportar = "";

            if (export.Equals("xls")) 
            {
                ViewBag.Exportar = "xls";
            }

            ViewBag.Periodo = Periodo;
            var proyectos = db.Proyecto.OrderBy(p => p.Nombre);
            return View(proyectos.ToList());
        }

        public ActionResult ResumenIntervenciones(int Periodo = 0, string export = "")
        {
            if (Periodo == 0)
            {
                Periodo = (int)Session["Periodo"];
            }

            ViewBag.Exportar = "";

            if (export.Equals("xls"))
            {
                ViewBag.Exportar = "xls";
            }

            ViewBag.Periodo = Periodo;
            var proyectos = db.Proyecto.OrderBy(p => p.Nombre);
            return View(proyectos.ToList());
        }

        public ActionResult Indicadores(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = (int)Session["Periodo"];
            }

            Proyecto proyecto = db.Proyecto.Find(((Proyecto)Session["Proyecto"]).ID);
            ViewBag.Periodo = Periodo;

            try
            {
                ViewBag.NombreProyecto = proyecto.Nombre;
                ViewBag.CodigoSename = proyecto.CodSename;
                ViewBag.CodigoCodeni = proyecto.CodCodeni;
                ViewBag.Ubicacion = proyecto.Direccion.Comuna.Nombre + ", " + proyecto.Direccion.Comuna.Region.Nombre;
                ViewBag.Auditor = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.ProyectoID == proyecto.ID).Where(r => r.TipoRolID == 4).Single().Persona.NombreCompleto;
            }
            catch (Exception)
            { }

            try
            {
                ViewBag.Indicadores = db.IndicadoresGestion.Where(i => i.ProyectoID == proyecto.ID && i.Periodo == Periodo).Single();
            }
            catch (Exception)
            {
                //ViewBag.Indicadores = new IndicadoresGestion();
                //http://localhost/SAG2/Control/Estandares/?Periodo=2011
                return RedirectToAction("Estandares", new { @Periodo = Periodo});
            }

            // Cobertura
            try
            {
                ViewBag.Cobertura_1 = db.Convenio.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 1 && m.Mes <= 3).Sum(m => m.NroPlazas);
            }
            catch (Exception)
            {
                ViewBag.Cobertura_1 = 0;
            }

            try
            {
                ViewBag.Cobertura_2 = db.Convenio.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 4 && m.Mes <= 6).Sum(m => m.NroPlazas);
            }
            catch (Exception)
            {
                ViewBag.Cobertura_2 = 0;
            }

            try
            {
                ViewBag.Cobertura_3 = db.Convenio.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 7 && m.Mes <= 9).Sum(m => m.NroPlazas);
            }
            catch (Exception)
            {
                ViewBag.Cobertura_3 = 0;
            }

            try
            {
                ViewBag.Cobertura_4 = db.Convenio.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 10 && m.Mes <= 12).Sum(m => m.NroPlazas);
            }
            catch (Exception)
            {
                ViewBag.Cobertura_4 = 0;
            }

            // Intervenciones

            try
            {
                ViewBag.Intervenciones_1 = db.Intervencion.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 1 && m.Mes <= 3).Sum(m => m.Atenciones);
            }
            catch (Exception)
            {
                ViewBag.Intervenciones_1 = 0;
            }

            try
            {
                ViewBag.Intervenciones_2 = db.Intervencion.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 4 && m.Mes <= 6).Sum(m => m.Atenciones);
            }
            catch (Exception)
            {
                ViewBag.Intervenciones_2 = 0;
            }

            try
            {
                ViewBag.Intervenciones_3 = db.Intervencion.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 7 && m.Mes <= 9).Sum(m => m.Atenciones);
            }
            catch (Exception)
            {
                ViewBag.Intervenciones_3 = 0;
            }

            try
            {
                ViewBag.Intervenciones_4 = db.Intervencion.Where(m => m.ProyectoID == proyecto.ID && m.Periodo == Periodo && m.Mes >= 10 && m.Mes <= 12).Sum(m => m.Atenciones);
            }
            catch (Exception)
            {
                ViewBag.Intervenciones_4 = 0;
            }


            // Ingresos
            try
            {
                ViewBag.Ingresos_Subvencion_1 = db.Movimiento.Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.CuentaID != 5).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Subvencion_1 = 0;
            }

            try
            {
                ViewBag.Ingresos_Subvencion_2 = db.Movimiento.Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.CuentaID != 5).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Subvencion_2 = 0;
            }
            
            try
            {
                ViewBag.Ingresos_Subvencion_3 = db.Movimiento.Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.CuentaID != 5).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Subvencion_3 = 0;
            }

            try
            {
                ViewBag.Ingresos_Subvencion_4 = db.Movimiento.Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.CuentaID != 5).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Subvencion_4 = 0;
            }

            try
            {
                ViewBag.Ingresos_Otros_1 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Otros_1 = 0;
            }

            try
            {
                ViewBag.Ingresos_Otros_2 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Otros_2 = 0;
            }

            try
            {
                ViewBag.Ingresos_Otros_3 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Otros_3 = 0;
            }

            try
            {
                ViewBag.Ingresos_Otros_4 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Otros_4 = 0;
            }

            try
            {
                ViewBag.Ingresos_Costos_1 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => !m.Cuenta.Codigo.StartsWith("5")).Where(m => !m.Cuenta.Codigo.StartsWith("4.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Costos_1 = 1;
            }

            try
            {
                ViewBag.Costos_Personal_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("6")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Personal_1 = 0;
            }

            try
            {
                ViewBag.Ingresos_Costos_2 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => !m.Cuenta.Codigo.StartsWith("5")).Where(m => !m.Cuenta.Codigo.StartsWith("4.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Costos_2 = 1;
            }

            try
            {
                ViewBag.Costos_Personal_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("6")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Personal_2 = 0;
            }

            try
            {
                ViewBag.Ingresos_Costos_3 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => !m.Cuenta.Codigo.StartsWith("5")).Where(m => !m.Cuenta.Codigo.StartsWith("4.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Costos_3 = 1;
            }

            try
            {
                ViewBag.Costos_Personal_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("6")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Personal_3 = 0;
            }

            try
            {
                ViewBag.Ingresos_Costos_4 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => !m.Cuenta.Codigo.StartsWith("5")).Where(m => !m.Cuenta.Codigo.StartsWith("4.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Ingresos_Costos_4 = 1;
            }

            try
            {
                ViewBag.Costos_Personal_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("6")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Personal_4 = 0;
            }

            // ***********************************
            // Funcionamiento
            // ***********************************

            try
            {
                ViewBag.Costos_Funcionamiento_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.1")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Funcionamiento_1 = 0;
            }

            try
            {
                ViewBag.Costos_Funcionamiento_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.1")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Funcionamiento_2 = 0;
            }

            try
            {
                ViewBag.Costos_Funcionamiento_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.1")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Funcionamiento_3 = 0;
            }

            try
            {
                ViewBag.Costos_Funcionamiento_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.1")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Funcionamiento_4 = 0;
            }

            // ***********************************
            // Apoyo Técnico
            // ***********************************

            try
            {
                ViewBag.Costos_ApoyoTecnico_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.2")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoTecnico_1 = 0;
            }

            try
            {
                ViewBag.Costos_ApoyoTecnico_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.2")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoTecnico_2 = 0;
            }

            try
            {
                ViewBag.Costos_ApoyoTecnico_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.2")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoTecnico_3 = 0;
            }

            try
            {
                ViewBag.Costos_ApoyoTecnico_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.2")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoTecnico_4 = 0;
            }

            // ***********************************
            // Apoyo a Beneficiarios
            // ***********************************

            try
            {
                ViewBag.Costos_ApoyoBeneficiarios_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.3")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoBeneficiarios_1 = 0;
            }

            try
            {
                ViewBag.Costos_ApoyoBeneficiarios_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.3")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoBeneficiarios_2 = 0;
            }

            try
            {
                ViewBag.Costos_ApoyoBeneficiarios_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.3")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoBeneficiarios_3 = 0;
            }

            try
            {
                ViewBag.Costos_ApoyoBeneficiarios_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("7.3")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_ApoyoBeneficiarios_4 = 0;
            }

            // ***********************************
            // Inversion
            // ***********************************

            try
            {
                ViewBag.Costos_Inversion_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("8")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Inversion_1 = 0;
            }

            try
            {
                ViewBag.Costos_Inversion_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("8")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Inversion_2 = 0;
            }

            try
            {
                ViewBag.Costos_Inversion_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("8")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Inversion_3 = 0;
            }

            try
            {
                ViewBag.Costos_Inversion_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("8")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Inversion_4 = 0;
            }

            // ***********************************
            // Indemnizacion
            // ***********************************

            try
            {
                ViewBag.Costos_Indemnizacion_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("9") || m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Indemnizacion_1 = 0;
            }

            try
            {
                ViewBag.Costos_Indemnizacion_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("9") || m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Indemnizacion_2 = 0;
            }

            try
            {
                ViewBag.Costos_Indemnizacion_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("9") || m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Indemnizacion_3 = 0;
            }

            try
            {
                ViewBag.Costos_Indemnizacion_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("E")).Where(m => m.Cuenta.Codigo.StartsWith("9") || m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Costos_Indemnizacion_4 = 0;
            }

            // ***********************************
            // Préstamos 
            // ***********************************

            try
            {
                ViewBag.Finan_Prestamos_1 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("5.1")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Prestamos_1 = 0;
            }

            try
            {
                ViewBag.Finan_Prestamos_2 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("5.1")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Prestamos_2 = 0;
            }

            try
            {
                ViewBag.Finan_Prestamos_3 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("5.1")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Prestamos_3 = 0;
            }

            try
            {
                ViewBag.Finan_Prestamos_4 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("5.1")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Prestamos_4 = 0;
            }

            // ***********************************
            // Aportes de Terceros 
            // ***********************************

            try
            {
                ViewBag.Finan_Aportes_1 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.1") || m.Cuenta.Codigo.StartsWith("5.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Aportes_1 = 0;
            }

            try
            {
                ViewBag.Finan_Aportes_2 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.1") || m.Cuenta.Codigo.StartsWith("5.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Aportes_2 = 0;
            }

            try
            {
                ViewBag.Finan_Aportes_3 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.1") || m.Cuenta.Codigo.StartsWith("5.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Aportes_3 = 0;
            }

            try
            {
                ViewBag.Finan_Aportes_4 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Cuenta.Tipo.Equals("I")).Where(m => m.Cuenta.Codigo.StartsWith("3.1") || m.Cuenta.Codigo.StartsWith("5.2")).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            {
                ViewBag.Finan_Aportes_4 = 0;
            }

            // ***********************************
            // Saldo Banco
            // ***********************************

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
                int saldoInicial = 0;
                try
                {
                    CuentaCorriente CuentaCorriente = db.CuentaCorriente.Where(c => c.ProyectoID == proyecto.ID).Single();
                    saldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == 1).Single().SaldoInicialCartola;
                }
                catch (Exception)
                { }
                ViewBag.Prod_SaldoBanco_1 = Ingresos - Egresos + saldoInicial;
            }
            catch (Exception)
            {
                ViewBag.Prod_SaldoBanco_1 = 0;
            }

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
                int saldoInicial = 0;
                try
                {
                    CuentaCorriente CuentaCorriente = db.CuentaCorriente.Where(c => c.ProyectoID == proyecto.ID).Single();
                    saldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == 4).Single().SaldoInicialCartola;
                }
                catch (Exception)
                { } 
                ViewBag.Prod_SaldoBanco_2 = Ingresos - Egresos + saldoInicial;
            }
            catch (Exception)
            {
                ViewBag.Prod_SaldoBanco_2 = 0;
            }

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
                int saldoInicial = 0;
                try
                {
                    CuentaCorriente CuentaCorriente = db.CuentaCorriente.Where(c => c.ProyectoID == proyecto.ID).Single();
                    saldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == 7).Single().SaldoInicialCartola;
                }
                catch (Exception)
                { } 
                ViewBag.Prod_SaldoBanco_3 = Ingresos - Egresos + saldoInicial;
            }
            catch (Exception)
            {
                ViewBag.Prod_SaldoBanco_3 = 0;
            }

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
                int saldoInicial = 0;
                try
                {
                    CuentaCorriente CuentaCorriente = db.CuentaCorriente.Where(c => c.ProyectoID == proyecto.ID).Single();
                    saldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes == 10).Single().SaldoInicialCartola;
                }
                catch (Exception)
                { } 
                ViewBag.Prod_SaldoBanco_4 = Ingresos - Egresos + saldoInicial;
            }
            catch (Exception)
            {
                ViewBag.Prod_SaldoBanco_4 = 0;
            }

            // ***********************************
            // Resultado
            // ***********************************

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
                ViewBag.Prod_Resultado_Ing_1 = Ingresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Ing_1 = 1;
            }

            try
            {
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => (m.Cuenta.Codigo.StartsWith("6.") || m.Cuenta.Codigo.StartsWith("7.")) && !m.Cuenta.Codigo.StartsWith("7.1.12")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
                ViewBag.Prod_Resultado_Egr_1 = Egresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Egr_1 = 0;
            }

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
                ViewBag.Prod_Resultado_Ing_2 = Ingresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Ing_2 = 1;
            }

            try
            {
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => (m.Cuenta.Codigo.StartsWith("6.") || m.Cuenta.Codigo.StartsWith("7.")) && !m.Cuenta.Codigo.StartsWith("7.1.12")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
                ViewBag.Prod_Resultado_Egr_2 = Egresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Egr_2 = 0;
            }

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
                ViewBag.Prod_Resultado_Ing_3 = Ingresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Ing_3 = 1;
            }

            try
            {
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => (m.Cuenta.Codigo.StartsWith("6.") || m.Cuenta.Codigo.StartsWith("7.")) && !m.Cuenta.Codigo.StartsWith("7.1.12")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
                ViewBag.Prod_Resultado_Egr_3 = Egresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Egr_3 = 0;
            }

            try
            {
                int Ingresos = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
                ViewBag.Prod_Resultado_Ing_4 = Ingresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Ing_4 = 1;
            }

            try
            {
                int Egresos = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => (m.Cuenta.Codigo.StartsWith("6.") || m.Cuenta.Codigo.StartsWith("7.")) && !m.Cuenta.Codigo.StartsWith("7.1.12")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
                ViewBag.Prod_Resultado_Egr_4 = Egresos;
            }
            catch (Exception)
            {
                ViewBag.Prod_Resultado_Egr_4 = 0;
            }

            // ***********************************
            // Proveedores
            // ***********************************

            try
            {
                ViewBag.Finan_Proveedores_1 = db.DeudaPendiente.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Finan_Proveedores_1 = 0;
            }

            try
            {
                ViewBag.Finan_Proveedores_2 = db.DeudaPendiente.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Finan_Proveedores_2 = 0;
            }

            try
            {
                ViewBag.Finan_Proveedores_3 = db.DeudaPendiente.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Finan_Proveedores_3 = 0;
            }

            try
            {
                ViewBag.Finan_Proveedores_4 = db.DeudaPendiente.Include(d => d.Cuenta).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.Finan_Proveedores_4 = 0;
            }

            // ***********************************
            // Aporte Subvención
            // ***********************************

            ViewBag.AporteSubvencion_Ing_1 = 1;
            ViewBag.AporteSubvencion_Ing_2 = 1;
            ViewBag.AporteSubvencion_Ing_3 = 1;
            ViewBag.AporteSubvencion_Ing_4 = 1;

            try
            {
                ViewBag.AporteSubvencion_1 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("7.1.13")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.AporteSubvencion_1 = 0;
            }

            try
            {
                ViewBag.AporteSubvencion_Ing_1 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 1).Where(m => m.Mes <= 3).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            { }

            try
            {
                ViewBag.AporteSubvencion_2 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("7.1.13")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.AporteSubvencion_2 = 0;
            }

            try
            {
                ViewBag.AporteSubvencion_Ing_2 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 4).Where(m => m.Mes <= 6).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            { }

            try
            {
                ViewBag.AporteSubvencion_3 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("7.1.13")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.AporteSubvencion_3 = 0;
            }

            try
            {
                ViewBag.AporteSubvencion_Ing_3 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 7).Where(m => m.Mes <= 9).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            { }

            try
            {
                ViewBag.AporteSubvencion_4 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("7.1.13")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
            }
            catch (Exception)
            {
                ViewBag.AporteSubvencion_4 = 0;
            }

            try
            {
                ViewBag.AporteSubvencion_Ing_4 = db.Movimiento.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("1.")).Where(m => m.ProyectoID == proyecto.ID).Where(m => m.Periodo == Periodo).Where(m => m.Mes >= 10).Where(m => m.Mes <= 12).Sum(m => m.Monto_Ingresos);
            }
            catch (Exception)
            { }
            	
            // ***********************************
            // Costo Niño/Mes
            // ***********************************
            /*
             * Se mide trimestralmente dividiendo los Gastos Totales (cuentas Nº 6, 7, 8, 9, y 10 (la 10 solo del último mes)) por los Ingresos Totales (cuentas Nº 1.1, 1.2 y 1.3)
             */

            ViewBag.CostoNiño_1 = 0;
            ViewBag.CostoNiño_2 = 0;
            ViewBag.CostoNiño_3 = 0;
            ViewBag.CostoNiño_4 = 0;

            try
            {
                int GastosTotales = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("6") || m.Cuenta.Codigo.StartsWith("7") || m.Cuenta.Codigo.StartsWith("8") || m.Cuenta.Codigo.StartsWith("9")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 1).Where(m => m.Egreso.Mes <= 3).Sum(m => m.Monto);
                ViewBag.CostoNiño_1 = ViewBag.CostoNiño_1 + GastosTotales;
            }
            catch (Exception)
            { }

            try
            {
                int GastosCta10 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes == 3).Sum(m => m.Monto);
                ViewBag.CostoNiño_1 = ViewBag.CostoNiño_1 + GastosCta10;
            }
            catch (Exception)
            { }

            try
            {
                int GastosTotales = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("6") || m.Cuenta.Codigo.StartsWith("7") || m.Cuenta.Codigo.StartsWith("8") || m.Cuenta.Codigo.StartsWith("9")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 4).Where(m => m.Egreso.Mes <= 6).Sum(m => m.Monto);
                ViewBag.CostoNiño_2 = GastosTotales + ViewBag.CostoNiño_2;
            }
            catch (Exception)
            { }

            try
            {
                int GastosCta10 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes == 6).Sum(m => m.Monto);
                ViewBag.CostoNiño_2 = ViewBag.CostoNiño_2 + GastosCta10;
            }
            catch (Exception)
            { }

            try
            {
                int GastosTotales = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("6") || m.Cuenta.Codigo.StartsWith("7") || m.Cuenta.Codigo.StartsWith("8") || m.Cuenta.Codigo.StartsWith("9")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 7).Where(m => m.Egreso.Mes <= 9).Sum(m => m.Monto);
                ViewBag.CostoNiño_3 = GastosTotales + ViewBag.CostoNiño_3;
            }
            catch (Exception)
            { }

            try
            {
                int GastosCta10 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes == 9).Sum(m => m.Monto);
                ViewBag.CostoNiño_3 = ViewBag.CostoNiño_3 + GastosCta10;
            }
            catch (Exception)
            { }

            try
            {
                int GastosTotales = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("6") || m.Cuenta.Codigo.StartsWith("7") || m.Cuenta.Codigo.StartsWith("8") || m.Cuenta.Codigo.StartsWith("9")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes >= 10).Where(m => m.Egreso.Mes <= 12).Sum(m => m.Monto);
                ViewBag.CostoNiño_4 = ViewBag.CostoNiño_4 + GastosTotales;
            }
            catch (Exception)
            { }

            try
            {
                int GastosCta10 = db.DetalleEgreso.Include(d => d.Cuenta).Where(m => m.Cuenta.Codigo.StartsWith("10")).Where(m => m.Egreso.ProyectoID == proyecto.ID).Where(m => m.Egreso.Periodo == Periodo).Where(m => m.Egreso.Mes == 12).Sum(m => m.Monto);
                ViewBag.CostoNiño_4 = ViewBag.CostoNiño_4 + GastosCta10;
            }
            catch (Exception)
            { }

            // ***********************************
            // Desviación del Presupuesto
            // ***********************************
            /*
             * Midiendo la desviación (+ ó -) de los Ingresos Presupuestados respecto de los Ingresos Reales del semestre (Total Ingresos Reales en las cuentas Nº 1.1, 1.2 y 1.3 (Menos) el Total de Ingresos Presupuestados de las cuentas Nº 1.1, 1.2 y 1.3 )  
             */

            ViewBag.Presupuesto_Ingresos_1 = 1;
            ViewBag.Presupuesto_Ingresos_2 = 1;

            ViewBag.Presupuesto_Egresos_1 = 1;
            ViewBag.Presupuesto_Egresos_2 = 1;

            List<DetallePresupuesto> dp = new List<DetallePresupuesto>();
            try
            {
                dp = db.DetallePresupuesto.Where(d => d.Presupuesto.ProyectoID == proyecto.ID && d.Periodo == Periodo).ToList();
            }
            catch(Exception)
            {}

            try
            {
                int IngresosPresupuesto = dp.Where(m => m.Cuenta.Codigo.StartsWith("1.") && m.Mes >= 1 && m.Mes <= 6).Sum(m => m.Monto);
                ViewBag.Presupuesto_Ingresos_1 = IngresosPresupuesto;
            }
            catch (Exception)
            { }

            try
            {
                int EgresosPresupuesto = dp.Where(m => m.Cuenta.Codigo.StartsWith("6") || m.Cuenta.Codigo.StartsWith("7") || m.Cuenta.Codigo.StartsWith("8") || m.Cuenta.Codigo.StartsWith("9") && m.Mes >= 1 && m.Mes <= 6).Sum(m => m.Monto);
                ViewBag.Presupuesto_Egresos_1 = ViewBag.Presupuesto_Egresos_1 + EgresosPresupuesto;
            }
            catch (Exception)
            { }

            try
            {
                int EgresosPresupuesto = dp.Where(m => m.Cuenta.Codigo.StartsWith("10") && m.Mes == 6).Sum(m => m.Monto);
                ViewBag.Presupuesto_Egresos_1 = ViewBag.Presupuesto_Egresos_1 + EgresosPresupuesto;
            }
            catch (Exception)
            { }

            try
            {
                int IngresosPresupuesto = dp.Where(m => m.Cuenta.Codigo.StartsWith("1.") && m.Mes >= 7 && m.Mes <= 12).Sum(m => m.Monto);
                ViewBag.Presupuesto_Ingresos_2 = IngresosPresupuesto;
            }
            catch (Exception)
            { }

            try
            {
                int EgresosPresupuesto = dp.Where(m => m.Cuenta.Codigo.StartsWith("6") || m.Cuenta.Codigo.StartsWith("7") || m.Cuenta.Codigo.StartsWith("8") || m.Cuenta.Codigo.StartsWith("9") && m.Mes >= 7 && m.Mes <= 12).Sum(m => m.Monto);
                ViewBag.Presupuesto_Egresos_2 = ViewBag.Presupuesto_Egresos_2 + EgresosPresupuesto;
            }
            catch (Exception)
            { }

            try
            {
                int EgresosPresupuesto = dp.Where(m => m.Cuenta.Codigo.StartsWith("10") && m.Mes == 12).Sum(m => m.Monto);
                ViewBag.Presupuesto_Egresos_2 = ViewBag.Presupuesto_Egresos_2 + EgresosPresupuesto;
            }
            catch (Exception)
            { }

            return View();
        }

        public ActionResult Estandares(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = (int)Session["Periodo"];
            }

            Proyecto proyecto = db.Proyecto.Find(((Proyecto)Session["Proyecto"]).ID);
            IndicadoresGestion Indicadores = new IndicadoresGestion();

            try
            {
                ViewBag.NombreProyecto = proyecto.Nombre;
                ViewBag.CodigoSename = proyecto.CodSename;
                ViewBag.CodigoCodeni = proyecto.CodCodeni;
                ViewBag.Periodo = Periodo;
                ViewBag.Ubicacion = proyecto.Direccion.Comuna.Nombre + ", " + proyecto.Direccion.Comuna.Region.Nombre;
                ViewBag.Auditor = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.ProyectoID == proyecto.ID).Where(r => r.TipoRolID == 4).Single().Persona.NombreCompleto;
            }
            catch (Exception)
            { }

            try
            {
                Indicadores = db.IndicadoresGestion.Where(i => i.ProyectoID == proyecto.ID && i.Periodo == Periodo).Single();
            }
            catch(Exception)
            {
                Indicadores.Periodo = Periodo;
                Indicadores.ProyectoID = proyecto.ID;
            }

            try
            {
                Indicadores.IngSubvencion = Int32.Parse(proyecto.ValorSubvencion.ToString()) * proyecto.Convenio.NroPlazas;
            }
            catch (Exception)
            {
                Indicadores.IngSubvencion = 0;
            }

            return View(Indicadores);
        }

        [HttpPost]
        public ActionResult Estandares(IndicadoresGestion Indicadores)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(Indicadores).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                if (ModelState.IsValid)
                {
                    db.IndicadoresGestion.Add(Indicadores);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Estandares");
        }
    }
}
