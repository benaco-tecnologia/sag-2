using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;
using SAG2.Classes;
using System.Data;

namespace SAG2.Controllers
{
    public class PresupuestoController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Constantes ctes = new Constantes();
        private Util utils = new Util();

        //
        // GET: /Presupuesto/Control
        public ActionResult ExportarExcel()
        {
            return Control();
        }

        public ActionResult Control(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.Mes_Inicio = "1";

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = 1;
                    int periodo = Periodo;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult ControlV2()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S")).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S")).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = Presupuesto.Mes_Inicio;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        //
        // GET: /Presupuesto/Formulacion

        public ActionResult Formulacion(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            ViewBag.PresupuestoID = string.Empty;
            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.Mes_Inicio = "1";
            List<DetallePresupuesto> dp = new List<DetallePresupuesto>();
            int activo = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).Count();

            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                ViewBag.Periodo_Inicio = Periodo.ToString();
                ViewBag.Mes_Inicio = "1";
                ViewBag.SaldoInicial = Presupuesto.SaldoInicial;

                dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();
            }
            catch (Exception)
            {
                //Response.Write(e.StackTrace);
                //Response.End();
            }

            ViewBag.Detalle = dp;
            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        [HttpPost]
        public ActionResult Formulacion(FormCollection form)
        {
            int presupuestoID;
            Presupuesto Presupuesto = null;
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            int mesInicio = Int32.Parse(form["MesInicioPresupuesto"].ToString());
            int periodoPresupuesto = Int32.Parse(form["periodoPresupuesto"].ToString());

            try
            {
                presupuestoID = Int32.Parse(form["PresupuestoID"].ToString());
                Presupuesto = db.Presupuesto.Find(presupuestoID);
                Presupuesto.Periodo = periodoPresupuesto;
                Presupuesto.Mes = mesInicio;
                Presupuesto.SaldoInicial = 0;

                if (form["SaldoInicial"] != null && !form["SaldoInicial"].ToString().Equals(""))
                {
                    Presupuesto.SaldoInicial = Int32.Parse(form["SaldoInicial"].ToString());
                }
                else
                {
                    Presupuesto.SaldoInicial = 0;
                }
                
                if (ModelState.IsValid)
                {
                    db.Entry(Presupuesto).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                Presupuesto = new Presupuesto();
                Presupuesto.Periodo = periodoPresupuesto;
                Presupuesto.Mes = mesInicio;
                Presupuesto.Activo = "S";
                Presupuesto.ProyectoID = Proyecto.ID;
                Presupuesto.SaldoInicial = Int32.Parse(form["SaldoInicial"].ToString());
                
                if (ModelState.IsValid)
                {
                    db.Presupuesto.Add(Presupuesto);
                    db.SaveChanges();
                }
            }


            db.Database.ExecuteSqlCommand("DELETE FROM DetallePresupuesto WHERE PresupuestoID = " + Presupuesto.ID);

            foreach (var key in form.AllKeys)
            {
                if (key.Contains("Presupuesto_") && key.Contains("_"))
                {
                    string[] datos = key.Split('_');
                    if ("Presupuesto".Equals(datos[0]) && datos.Count() == 3)
                    {
                        //Presupuesto_7_3
                        int monto = Int32.Parse(form[key].ToString());

                        if (monto == 0)
                        {
                            continue;
                        }

                        DetallePresupuesto dp = new DetallePresupuesto();
                        int mes = Int32.Parse(datos[1]) - 1 + Presupuesto.Mes;
                        int periodo = Presupuesto.Periodo;

                        if (mes > 12)
                        {
                            mes = mes - 12;
                            periodo = periodo + 1;
                        }

                        int cuentaID = Int32.Parse(datos[2]);

                        /*
                        try
                        {
                            dp = db.DetallePresupuesto.Where(m => m.PresupuestoID == Presupuesto.ID).Where(m => m.CuentaID == cuentaID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Single();
                            dp.Monto = monto;

                            if (ModelState.IsValid)
                            {
                                db.Entry(dp).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {
                        */
                            dp.PresupuestoID = Presupuesto.ID;
                            dp.CuentaID = cuentaID;
                            dp.Periodo = periodo;
                            dp.Mes = mes;
                            dp.Monto = monto;

                            db.DetallePresupuesto.Add(dp);
                            db.SaveChanges();
                        //}
                    }
                }
            }

            return RedirectToAction("Formulacion", new { Periodo = periodoPresupuesto });
        }

        public ActionResult Excel(int Periodo = 0) 
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = Presupuesto.Mes_Inicio;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult ExcelSemestre2(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];

            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                // Obtenemos saldo inicial para Julio (Inicio segundo semestre)
                int añoPresupuesto = Presupuesto.Periodo;
                try
                {
                    ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 7 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                }
                catch (Exception)
                {
                    try
                    {
                        ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 6 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 5 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 4 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 3 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 2 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                                    }
                                    catch (Exception)
                                    {
                                        try
                                        {
                                            ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 1 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                                        }
                                        catch (Exception)
                                        {
                                            ViewBag.SaldoJulio = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    //ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.Mes_Inicio = 7;
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    //int mes = Presupuesto.Mes_Inicio;
                    int mes = 7;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();
                    /*
                    for (int i = 0; i < 6; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                            continue;
                        }
                        mes++;
                    }*/

                    if (mes <= 6)
                    { 
                        mes = mes + 6;
                    }
                    else if (mes == 7)
                    {
                        mes = 1;
                        periodo = periodo + 1;
                    }
                    else if (mes == 8)
                    {
                        mes = 2;
                        periodo = periodo + 1;
                    }
                    else if (mes == 9)
                    {
                        mes = 3;
                        periodo = periodo + 1;
                    }
                    else if (mes == 10)
                    {
                        mes = 4;
                        periodo = periodo + 1;
                    }
                    else if (mes == 11)
                    {
                        mes = 5;
                        periodo = periodo + 1;
                    }
                    else if (mes == 12)
                    {
                        mes = 6;
                        periodo = periodo + 1;
                    }

                    mes = 6;
                    ViewBag.Mes_Inicio = mes;

                    for (int i = 0; i < 6; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult ExcelSemestre1(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    //ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.Mes_Inicio = 1;
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    //int mes = Presupuesto.Mes_Inicio;
                    int mes = 1;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 6; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult ExcelBalance(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = Presupuesto.Mes_Inicio;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult ExcelBalanceLineaResponsabilidad(int Periodo = 0, int Linea = 5)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = Presupuesto.Mes_Inicio;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }


        public ActionResult ExcelBalanceSemestre1(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = Presupuesto.Mes_Inicio;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult ExcelBalanceSemestre2(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            int añoPresupuesto = Periodo;
            try
            {
                ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 7 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
            }
            catch (Exception)
            {
                try
                {
                    ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 6 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                }
                catch (Exception)
                {
                    try
                    {
                        ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 5 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 4 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 3 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 2 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        ViewBag.SaldoJulio = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID && s.Mes == 1 && s.Periodo == añoPresupuesto).Single().SaldoInicialCartola;
                                    }
                                    catch (Exception)
                                    {
                                        ViewBag.SaldoJulio = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Presupuesto.Periodo_Inicio.ToString();
                    ViewBag.Mes_Inicio = Presupuesto.Mes_Inicio.ToString();
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = Presupuesto.Mes_Inicio;
                    int periodo = Presupuesto.Periodo_Inicio;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null && m.Egreso.Temporal == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception e)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + e.StackTrace);
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception ex)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado. " + ex.StackTrace);
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult Graficos()
        {
            return View();
        }

        public ActionResult Balance(int Periodo = 0)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.Mes_Inicio = "1";

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
                */
                ViewBag.Ingresos = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();
                ViewBag.Reintegros = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

                /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
                */
                ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.ProyectoID == Proyecto.ID && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                   
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    int mes = 1;
                    int periodo = Periodo;

                    List<int> Ingresos = new List<int>();
                    List<int> Egresos = new List<int>();
                    List<int> Reintegros = new List<int>();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        try
                        {
                            Ingresos.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Ingresos.Add(0);
                        }

                        try
                        {
                            Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            Egresos.Add(0);
                        }

                        try
                        {
                            Reintegros.Add(db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                        }
                        catch (Exception)
                        {
                            Reintegros.Add(0);
                        }

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.MovIngresos = Ingresos;
                    ViewBag.MovEgresos = Egresos;
                    ViewBag.MovReintegros = Reintegros;

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult LineaProteccion(int Periodo = 0, int Linea = 5)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.LineaAtencion = Linea.ToString();

            //Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
            */
            ViewBag.Ingresos = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();
            ViewBag.Reintegros = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

            /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
            */
            ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

            List<int> Ingresos = new List<int>();
            List<int> Egresos = new List<int>();
            List<int> Reintegros = new List<int>();

            int mes = 1;
            int periodo = Periodo;

            for (int i = 0; i < 12; i++)
            {
                try
                {
                    Ingresos.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Ingresos.Add(0);
                }

                try
                {
                    Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                }
                catch (Exception)
                {
                    Egresos.Add(0);
                }

                try
                {
                    Reintegros.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Reintegros.Add(0);
                }
            }

            ViewBag.MovIngresos = Ingresos;
            ViewBag.MovEgresos = Egresos;
            ViewBag.MovReintegros = Reintegros;

            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();

                
                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Periodo.ToString();
                    ViewBag.Mes_Inicio = "1";
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }

                        

                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult LineaProteccionExcel(int Periodo = 0, int Linea = 5)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.LineaAtencion = Linea.ToString();

            //Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
            */
            ViewBag.Ingresos = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();
            ViewBag.Reintegros = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

            /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
            */
            ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

            List<int> Ingresos = new List<int>();
            List<int> Egresos = new List<int>();
            List<int> Reintegros = new List<int>();

            int mes = 1;
            int periodo = Periodo;

            for (int i = 0; i < 12; i++)
            {
                try
                {
                    Ingresos.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Ingresos.Add(0);
                }

                try
                {
                    Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                }
                catch (Exception)
                {
                    Egresos.Add(0);
                }

                try
                {
                    Reintegros.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Reintegros.Add(0);
                }
            }

            ViewBag.MovIngresos = Ingresos;
            ViewBag.MovEgresos = Egresos;
            ViewBag.MovReintegros = Reintegros;

            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();


                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Periodo.ToString();
                    ViewBag.Mes_Inicio = "1";
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }



                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult LineaResponsabilidad(int Periodo = 0, int Linea = 9)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.LineaAtencion = Linea.ToString();

            //Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
            */
            ViewBag.Ingresos = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();
            ViewBag.Reintegros = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

            /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
            */
            ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

            List<int> Ingresos = new List<int>();
            List<int> Egresos = new List<int>();
            List<int> Reintegros = new List<int>();

            int mes = 1;
            int periodo = Periodo;

            for (int i = 0; i < 12; i++)
            {
                try
                {
                    Ingresos.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Ingresos.Add(0);
                }

                try
                {
                    Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                }
                catch (Exception)
                {
                    Egresos.Add(0);
                }

                try
                {
                    Reintegros.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Reintegros.Add(0);
                }
            }

            ViewBag.MovIngresos = Ingresos;
            ViewBag.MovEgresos = Egresos;
            ViewBag.MovReintegros = Reintegros;

            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();


                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Periodo.ToString();
                    ViewBag.Mes_Inicio = "1";
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }



                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

        public ActionResult LineaResponsabilidadExcel(int Periodo = 0, int Linea = 9)
        {
            if (Periodo == 0)
            {
                Periodo = DateTime.Now.Year;
            }

            ViewBag.Periodo_Inicio = Periodo.ToString();
            ViewBag.LineaAtencion = Linea.ToString();

            //Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            /*
                SELECT cuentaid, periodo, mes, SUM(monto_ingresos)
                FROM Movimiento
                where CuentaID is not null and Nulo is null
                group by periodo, mes, cuentaID
            */
            ViewBag.Ingresos = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.Nulo == null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).OrderBy(m => m.Cuenta.Orden).ToList();
            ViewBag.Reintegros = db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null && m.Temporal == null).OrderBy(m => m.Cuenta.Orden).ToList();

            /*
                SELECT cuentaid, SUM(monto)
                FROM DetalleEgreso
                where CuentaID is not null and Nulo is null
                and MovimientoID in (select ID from Movimiento where Nulo is null)
                group by cuentaID
            */
            ViewBag.Egresos = db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).OrderBy(m => m.Cuenta.Orden).ToList();

            List<int> Ingresos = new List<int>();
            List<int> Egresos = new List<int>();
            List<int> Reintegros = new List<int>();

            int mes = 1;
            int periodo = Periodo;

            for (int i = 0; i < 12; i++)
            {
                try
                {
                    Ingresos.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoIngreso).Where(m => m.CuentaID != null && m.CuentaID != 1 && m.Nulo == null && m.Eliminado == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Ingresos.Add(0);
                }

                try
                {
                    Egresos.Add(db.DetalleEgreso.Where(m => m.Egreso.Proyecto.TipoProyectoID == Linea).Where(m => m.Egreso.Periodo == periodo).Where(m => m.Egreso.Mes == mes).Where(m => m.CuentaID != null).Where(m => m.Nulo == null && m.Egreso.Eliminado == null).Sum(m => m.Monto));
                }
                catch (Exception)
                {
                    Egresos.Add(0);
                }

                try
                {
                    Reintegros.Add(db.Movimiento.Where(m => m.Proyecto.TipoProyectoID == Linea).Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.CuentaID != null && m.Eliminado == null && m.CuentaID != 1 && m.Nulo == null).Sum(m => m.Monto_Ingresos));
                }
                catch (Exception)
                {
                    Reintegros.Add(0);
                }
            }

            ViewBag.MovIngresos = Ingresos;
            ViewBag.MovEgresos = Egresos;
            ViewBag.MovReintegros = Reintegros;

            try
            {
                Presupuesto Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();


                List<DetallePresupuesto> dp = new List<DetallePresupuesto>();

                try
                {
                    Presupuesto = db.Presupuesto.Where(m => m.Proyecto.TipoProyectoID == Linea && m.Activo != null && m.Activo.Equals("S") && m.Periodo == Periodo).OrderByDescending(p => p.ID).Take(1).Single();
                    ViewBag.PresupuestoID = Presupuesto.ID.ToString();
                    ViewBag.Periodo_Inicio = Periodo.ToString();
                    ViewBag.Mes_Inicio = "1";
                    ViewBag.SaldoInicial = Presupuesto.SaldoInicial;
                    dp = db.DetallePresupuesto.Where(d => d.PresupuestoID == Presupuesto.ID).ToList();

                    List<int> IngresosPre = new List<int>();
                    List<int> EgresosPre = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        if (mes > 12)
                        {
                            mes = 1;
                            periodo++;
                        }



                        try
                        {
                            IngresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("I")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            IngresosPre.Add(0);
                        }

                        try
                        {
                            EgresosPre.Add(dp.Where(d => d.Cuenta.Tipo.Equals("E")).Where(d => d.Mes == mes).Where(d => d.Periodo == periodo).Sum(m => m.Monto));
                        }
                        catch (Exception)
                        {
                            EgresosPre.Add(0);
                        }

                        mes++;
                    }

                    ViewBag.PreIngresos = IngresosPre;
                    ViewBag.PreEgresos = EgresosPre;
                }
                catch (Exception)
                {
                    ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
                }

                ViewBag.Detalle = dp;
            }
            catch (Exception)
            {
                ViewBag.NoHayPresupuesto = utils.mensajeError("Para poder ver el control debe existir un Presupuesto formulado.");
            }

            var cuenta = db.Cuenta.Where(c => !c.Codigo.Equals("0") && !c.Codigo.Equals("7.3.9")).OrderBy(c => c.Orden);
            return View(cuenta.ToList());
        }

    }
}
