using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;
using SAG2.Classes;

namespace SAG2.Controllers
{
    public class InformesController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Constantes ctes = new Constantes();

        //
        // GET: /Informes/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RegistroSemanalPago()
        {
            DateTime inicio;
            DateTime fin;
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];

            if (Request.QueryString["i"] != null && Request.QueryString["f"] != null && Request.QueryString["s"] != null)
            {
                @ViewBag.Fecha = Request.QueryString["s"].ToString();
                inicio = DateTime.Parse(Request.QueryString["i"].ToString());
                fin = DateTime.Parse(Request.QueryString["f"].ToString()).AddDays(1);
            }
            else
            {
                DateTime Fecha = new DateTime();
                try
                {
                    Fecha = new DateTime((int)Session["Periodo"], (int)Session["Mes"], DateTime.Now.Day);
                }
                catch (Exception)
                {
                    try
                    {
                        Fecha = new DateTime((int)Session["Periodo"], (int)Session["Mes"], 30);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Fecha = new DateTime((int)Session["Periodo"], (int)Session["Mes"], 29);
                        }
                        catch (Exception)
                        {
                            Fecha = new DateTime((int)Session["Periodo"], (int)Session["Mes"], 28);
                        }
                    }
                }
                @ViewBag.Fecha = Fecha.ToShortDateString();
                // Si semana empieza el domingo, agregar 2
                int add = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["addDays"]);
                int rm = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["rmDays"]);
                DateTime to = DateTime.Now.AddDays(add); // stores today date
                int x = (int)to.DayOfWeek; // gets the day of this week
                inicio = to.AddDays(-x - rm); // get the start date of this week
                fin = to.AddDays(6 - x - rm); // get the end date of this week
            }

            List<Movimiento> Ingresos = db.Movimiento.Where(m => m.Fecha >= inicio).Where(m => m.Fecha <= fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID != ctes.tipoEgreso).Where(m => m.CuentaID != 1 && m.Eliminado == null && m.Temporal == null).ToList();
            List<Movimiento> Egresos = db.Movimiento.Where(m => m.Fecha >= inicio).Where(m => m.Fecha <= fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoEgreso).Where(m => m.Temporal == null && m.Eliminado == null && ((m.CuentaID != 6 || m.CuentaID == null) || m.CuentaID == null)).ToList();
            @ViewBag.Ingresos_Reintegros = Ingresos;
            @ViewBag.Egresos = Egresos;
            ViewBag.total_ingresos = Ingresos.Sum(i => i.Monto_Ingresos);
            ViewBag.total_egresos = Egresos.Sum(e => e.Monto_Egresos);
            ViewBag.Monto_Ingresos = 0;
            ViewBag.Monto_Egresos = 0;
            ViewBag.SaldoFinal = 0;

            try
            {
                DateTime inicioTmp = new DateTime(inicio.Year, inicio.Month, 1);

                //DateTime inicioTmp = DateTime.Parse("01-" + inicio.Month + "-" + inicio.Year);
                //inicio = inicioTmp;
                //int ingresos = db.Movimiento.Where(m => m.Fecha >= inicio).Where(m => m.Fecha <= fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Nulo == null && m.CuentaID != 1 && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos);
                //int egresos = db.Movimiento.Where(m => m.Fecha >= inicio).Where(m => m.Fecha <= fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Nulo == null && m.Temporal == null && m.Eliminado == null && ((m.CuentaID != 6 || m.CuentaID == null) || m.CuentaID == null)).Sum(m => m.Monto_Egresos);
                //Movimiento mv = db.Movimiento.Where(m => m.Fecha >= inicio).Where(m => m.Fecha <= fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Nulo == null).Where(a => a.Temporal == null).OrderBy(m => m.ID).Take(1).Single();

                List<Movimiento> Ingresos_t = db.Movimiento.Where(m => m.Fecha >= inicioTmp).Where(m => m.Fecha < inicio).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID != ctes.tipoEgreso).Where(m => m.Nulo == null && m.CuentaID != 1 && m.Eliminado == null && m.Temporal == null).ToList();
                List<Movimiento> Egresos_t = db.Movimiento.Where(m => m.Fecha >= inicioTmp).Where(m => m.Fecha < inicio).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoEgreso).Where(m => m.Nulo == null && m.Temporal == null && m.Eliminado == null && ((m.CuentaID != 6 || m.CuentaID == null) || m.CuentaID == null)).ToList();


                ViewBag.Monto_Ingresos = Ingresos_t.Sum(i => i.Monto_Ingresos); ;
                ViewBag.Monto_Egresos = Egresos_t.Sum(e => e.Monto_Egresos);
                //ViewBag.SaldoFinal = ingresos - egresos;//mv.Saldo;
            }
            catch
            { }

            ViewBag.SaldoPeriodo = 0;

            // Obtenemos saldo hasta la semana seleccionada
            int periodo = inicio.Year;
            int mes = inicio.Month;
            int saldoInicial = 0;

            try
            {
                saldoInicial = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID && m.Periodo == periodo & m.Mes == mes).Single().SaldoInicialCartola;
            }
            catch (Exception)
            { 
                // Error, no ha saldo para el periodo
                try
                {
                    Saldo Saldo = db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Single();
                    if (periodo == Saldo.Periodo && mes > Saldo.Mes)
                    {
                        saldoInicial = Saldo.SaldoInicialCartola;
                    }
                }
                catch (Exception)
                { }
            }

            try
            { 
                
                int ingresos = db.Movimiento.Where(m => m.Fecha < inicio && m.Periodo == periodo && m.Mes == mes).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Nulo == null && m.CuentaID != 1 && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos);
                int egresos = db.Movimiento.Where(m => m.Fecha < inicio && m.Periodo == periodo && m.Mes == mes).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Nulo == null && m.Temporal == null && m.Eliminado == null && ((m.CuentaID != 6 || m.CuentaID == null) || m.CuentaID == null)).Sum(m => m.Monto_Egresos);
                ViewBag.SaldoPeriodo = ingresos - egresos + db.Saldo.Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Single().SaldoInicialCartola;
            }
            catch (Exception)
            {
                //Response.Write(e.Message + " "+e.StackTrace);
            }

            ViewBag.SaldoInicial = saldoInicial;
            return View();
        }

        [HttpGet]
        public ActionResult Ingresos(int Periodo = 0, int Mes = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo != 0 && Mes != 0)
            {
                int mes = Mes;
                int año = Periodo;
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();

                ViewBag.Rendicion = "Rendicion";
                var ingresos = db.Movimiento.Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
            else
            {
                int mes = (int)Session["Mes"];
                int año = (int)Session["Periodo"];
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                var ingresos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso && m.CuentaID != 1 && m.Eliminado == null && m.Temporal == null).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
        }

        [HttpPost]
        public ActionResult Ingresos(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                var ingresos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
            else
            {
                int Periodo = (int)Session["Periodo"];
                int Mes = (int)Session["Mes"];
                int mes = Mes;
                int año = Periodo;
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                var ingresos = db.Movimiento.Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
        }

        public ActionResult Reintegros(int Periodo = 0, int Mes = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo != 0 && Mes != 0)
            {
                int mes = Mes;
                int año = Periodo;
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                ViewBag.Rendicion = "Rendicion";
                var ingresos = db.Movimiento.Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);;
                return View(ingresos.ToList());
            }
            else
            {
                int mes = (int)Session["Mes"];
                int año = (int)Session["Periodo"];
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                var ingresos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
        }

        [HttpPost]
        public ActionResult Reintegros(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                var ingresos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
            else
            {
                int Periodo = (int)Session["Periodo"];
                int Mes = (int)Session["Mes"];
                int mes = Mes;
                int año = Periodo;
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                var ingresos = db.Movimiento.Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(ingresos.ToList());
            }
        }


        /*Revisa como se producen*/
        public ActionResult Egresos(int Periodo = 0, int Mes = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo != 0 && Mes != 0)
            {
                int mes = Mes;
                int año = Periodo;
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                ViewBag.Rendicion = "Rendicion";
                //var movimientos = db.Movimiento.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null).OrderBy(m => m.Fecha);
                var movimientos = db.Movimiento.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(movimientos.ToList());
                //var egresos = db.DetalleEgreso.Where(m => m.Egreso.Mes >= Mes).Where(m => m.Egreso.Periodo <= Periodo).Where(m => m.Egreso.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                //return View(egresos.ToList());
            }
            else
            {
                int mes = (int)Session["Mes"];
                int año = (int)Session["Periodo"];
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                //var movimientos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null).OrderBy(m => m.Fecha);
                var movimientos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(movimientos.ToList());
                //var egresos = db.DetalleEgreso.Where(m => m.Egreso.Fecha >= Inicio).Where(m => m.Egreso.Fecha <= Fin).Where(m => m.Egreso.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                //return View(egresos.ToList());
            }
        }

        [HttpPost]
        public ActionResult Egresos(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                var movimientos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(movimientos.ToList());
                //var egresos = db.DetalleEgreso.Where(m => m.Egreso.Fecha >= Inicio).Where(m => m.Egreso.Fecha <= Fin).Where(m => m.Egreso.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                //return View(egresos.ToList());
            }
            else
            {
                int Periodo = (int)Session["Periodo"];
                int Mes = (int)Session["Mes"];
                int mes = Mes;
                int año = Periodo;
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                var movimientos = db.Movimiento.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
                return View(movimientos.ToList());
                //var egresos = db.DetalleEgreso.Where(m => m.Egreso.Mes >= Mes).Where(m => m.Egreso.Periodo <= Periodo).Where(m => m.Egreso.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                //return View(egresos.ToList());
            }
        }

        public ActionResult DeudasPendientes(int Periodo = 0, int Mes = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            ViewBag.Clasificacion = "Todos";
            if (Periodo != 0 && Mes != 0)
            {
                ViewBag.Rendicion = "Rendicion";
                var dp = db.DeudaPendiente.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID && m.CuentaID != 1).OrderBy(m => m.NumeroComprobante);
                return View(dp.ToList());
            }
            else
            {
                int mes = (int)Session["Mes"];
                int año = (int)Session["Periodo"];
                int numberOfDays = DateTime.DaysInMonth(año, mes);
                DateTime Inicio = new DateTime(año, mes, 1);
                DateTime Fin = new DateTime(año, mes, numberOfDays);
                ViewBag.Desde = Inicio.ToShortDateString();
                ViewBag.Hasta = Fin.ToShortDateString();
                var dp = db.DeudaPendiente.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID && m.CuentaID != 1).OrderBy(m => m.ID);
                return View(dp.ToList());
            }
        }

        [HttpPost]
        public ActionResult DeudasPendientes(string Desde = "", string Hasta = "", string Clasificacion = "Todos")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            ViewBag.Clasificacion = Clasificacion;
            if (!Desde.Equals("") && !Hasta.Equals(""))
            {
                DateTime Inicio = DateTime.Parse(Desde);
                DateTime Fin = DateTime.Parse(Hasta);
                ViewBag.Desde = Desde;
                ViewBag.Hasta = Hasta;
                if (Clasificacion.Equals("Cancelados"))
                {
                    var dp = db.DeudaPendiente.Where(m => m.EgresoID != null).Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID && m.CuentaID != 1).OrderBy(m => m.ID);
                    return View(dp.ToList());
                }
                else if (Clasificacion.Equals("NoCancelados"))
                {
                    var dp = db.DeudaPendiente.Where(m => m.EgresoID == null).Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID && m.CuentaID != 1).OrderBy(m => m.ID);
                    return View(dp.ToList());
                }
                else
                {
                    var dp = db.DeudaPendiente.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                    return View(dp.ToList());
                }
            }
            else
            {
                int Periodo = (int)Session["Periodo"];
                int Mes = (int)Session["Mes"];
                if (Clasificacion.Equals("Cancelados"))
                {
                    var dp = db.DeudaPendiente.Where(m => m.EgresoID != null).Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                    return View(dp.ToList());
                }
                else if (Clasificacion.Equals("NoCancelados"))
                {
                    var dp = db.DeudaPendiente.Where(m => m.EgresoID == null).Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                    return View(dp.ToList());
                }
                else
                {
                    var dp = db.DeudaPendiente.Where(m => m.Mes >= Mes).Where(m => m.Periodo <= Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
                    return View(dp.ToList());
                }
            }
        }

        public ActionResult Honorarios(int Periodo = 0, int Mes = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo != 0 && Mes != 0)
            {
                ViewBag.Rendicion = "Rendicion";
            }
            else
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.Periodo = Periodo;
            ViewBag.Mes = Mes;
            var bh = db.BoletaHonorario.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
            return View(bh.ToList());
        }

        [HttpPost]
        public ActionResult Honorarios(int Periodo = 0, int Mes = 0, int flag = 1)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo == 0 && Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.Periodo = Periodo;
            ViewBag.Mes = Mes;
            var bh = db.BoletaHonorario.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
            return View(bh.ToList());
        }

        public ActionResult FondoFijo(int Periodo = 0, int Mes = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo != 0 && Mes != 0)
            {
                ViewBag.Rendicion = "Rendicion";
            }
            else
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.Periodo = Periodo;
            ViewBag.Mes = Mes;
            var ff = db.FondoFijo.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
            return View(ff.ToList());
        }

        [HttpPost]
        public ActionResult FondoFijo(int Periodo = 0, int Mes = 0, int flag = 1)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (Periodo == 0 && Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.Periodo = Periodo;
            ViewBag.Mes = Mes;
            var ff = db.FondoFijo.Where(m => m.Mes == Mes).Where(m => m.Periodo == Periodo).Where(m => m.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
            return View(ff.ToList());
        }

        public ActionResult ExcelIngresos(string Desde = "", string Hasta = "")
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
                var ingresos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderBy(m => m.NumeroComprobante);
                return View(ingresos.ToList());
            }

            return null;
        }

        public ActionResult ExcelReintegros(string Desde = "", string Hasta = "")
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
                var reintegros = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderBy(m => m.NumeroComprobante);
                return View(reintegros.ToList());
            }

            return null;
        }

        public ActionResult ExcelEgresos(string Desde = "", string Hasta = "")
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            DateTime Inicio = DateTime.Parse(Desde);
            DateTime Fin = DateTime.Parse(Hasta);
            ViewBag.Desde = Desde;
            ViewBag.Hasta = Hasta;
            var movimientos = db.Movimiento.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == 2).Where(a => a.Temporal == null && a.Eliminado == null && (a.CuentaID != 6 || a.CuentaID == null)).OrderByDescending(a => a.Periodo).ThenBy(a => a.NumeroComprobante);
            return View(movimientos.ToList());
            //var egresos = db.DetalleEgreso.Where(m => m.Egreso.Fecha >= Inicio).Where(m => m.Egreso.Fecha <= Fin).Where(m => m.Egreso.ProyectoID == Proyecto.ID).OrderBy(m => m.ID);
            //return View(egresos.ToList());
        }

        public ActionResult ExcelFondoFijo(int Mes = 0, int Periodo = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Mes = Mes;
            @ViewBag.Periodo = Periodo;
           
            if (Mes > 0 && Periodo > 0)
            {
                var fondofijo = db.FondoFijo.Where(f => f.ProyectoID == Proyecto.ID).Where(f => f.Mes == Mes).Where(f => f.Periodo == Periodo).OrderBy(f => f.Cuenta.Orden);
                return View(fondofijo.ToList());
            }

            return null;
        }

        public ActionResult ExcelHonorarios(int Mes = 0, int Periodo = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            @ViewBag.Proyecto = Proyecto.NombreLista;
            @ViewBag.Mes = Mes;
            @ViewBag.Periodo = Periodo;

            if (Mes > 0 && Periodo > 0)
            {
                var boletahonorario = db.BoletaHonorario.Where(f => f.ProyectoID == Proyecto.ID).Where(b => b.Mes == Mes).Where(b => b.Periodo == Periodo);
                return View(boletahonorario.ToList());
            }

            return null;
        }

        public ActionResult ExcelDeudasPendientes(string Desde = "", string Hasta = "")
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
                var deudapendientes = db.DeudaPendiente.Where(m => m.Fecha >= Inicio).Where(m => m.Fecha <= Fin).Where(m => m.ProyectoID == Proyecto.ID && m.CuentaID != 1).OrderBy(d => d.Cuenta.Orden);
                return View(deudapendientes.ToList());
            }

            return null;
        }
    }
}