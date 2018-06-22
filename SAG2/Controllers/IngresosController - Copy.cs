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
using System.Data.Objects;

namespace SAG2.Controllers
{ 
    public class IngresosController2 : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Util utils = new Util();
        //
        // GET: /Ingresos/

        public ViewResult Index(string q = "")
        {
            var movimiento = db.Movimiento.Include(m => m.Establecimiento).Include(m => m.TipoComprobante).Include(m => m.Cuenta).Include(m => m.Beneficiario).Include(m => m.CuentaCorriente).Include(m => m.ComprobanteEgreso).Where(a => a.TipoComprobanteID == 1).OrderByDescending(a => a.NumeroComprobante);
            return View(movimiento.ToList());
        }

        public ViewResult Informe()
        {
            Proyecto Establecimiento = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];

            @ViewBag.Proyecto = Establecimiento;
            @ViewBag.CuentaCorriente = CuentaCorriente;

            return View(db.Movimiento.Include(m => m.Establecimiento).
                                      Include(m => m.TipoComprobante).
                                      Include(m => m.Cuenta).
                                      Include(m => m.Beneficiario).
                                      Include(m => m.CuentaCorriente).
                                          Where(m => m.CuentaCorrienteID == CuentaCorriente.ID).
                                          Where(m => m.EstablecimientoID == Establecimiento.ID).ToList());
        }

        //
        // GET: /Ingresos/Details/5

        public ViewResult Details(int id)
        {
            Movimiento movimiento = db.Movimiento.Find(id);
            return View(movimiento);
        }

        //
        // GET: /Ingresos/Create

        public ActionResult Create()
        {
            int periodo = (int)Session["Periodo"];
            ViewBag.NroComprobante = "1";

            if (db.Movimiento.Where(a => a.TipoComprobanteID == 1).Where(a => a.Periodo == periodo).Count() > 0)
            {
                ViewBag.NroComprobante = db.Movimiento.Where(a => a.TipoComprobanteID == 1).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
            }

            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ID_CUENTA_INGRESO"])));
            return View();
        } 

        //
        // POST: /Ingresos/Create

        [HttpPost]
        public ActionResult Create(Movimiento movimiento)
        {
            utils.Log(1, "Inicio proceso de ingreso");

            int saldoFinal = 0;
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            int cuentaID = Int32.Parse(Request.Form["CuentaID"].ToString());
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            movimiento.NumeroComprobante = 1;

            if (db.Movimiento.Where(a => a.TipoComprobanteID == 1).Where(a => a.Periodo == periodo).Count() > 0)
            {
                movimiento.NumeroComprobante = db.Movimiento.Where(a => a.TipoComprobanteID == 1).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
            }

            movimiento.EstablecimientoID = Proyecto.ID;
            movimiento.CuentaCorrienteID = CuentaCorriente.ID;
            movimiento.Mes = (int)Session["Mes"];
            movimiento.Periodo = (int)Session["Periodo"];
            movimiento.BeneficiarioID = 2;
            movimiento.ComprobanteEgresoID = 1;
            movimiento.TipoComprobanteID = 1;
            movimiento.CuentaID = cuentaID;
            ViewBag.NroComprobante = movimiento.NumeroComprobante.ToString();
            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ID_CUENTA_INGRESO"])), cuentaID);
            movimiento.Saldo = saldoFinal;

            try
            {
                if (ModelState.IsValid)
                {
                    db.Movimiento.Add(movimiento);
                    db.SaveChanges();
                    @ViewBag.Mensaje = utils.mensajeOK("Ingreso registrado con éxito!");
                }
                else
                {
                    @ViewBag.Mensaje = utils.mensajeError("Ocurrió un error al registrar el ingreso");
                    utils.erroresState(ModelState);
                    return View(movimiento);
                }
            }
            catch (SqlException e)
            {
                @ViewBag.Mensaje = utils.mensajeError(e.Message);
                return View(movimiento);
            }
            catch (Exception e)
            {
                @ViewBag.Mensaje = utils.mensajeError(e.Message);
                return View(movimiento);
            }
            
            utils.Log(1, "Fin proceso de ingreso");
            return View(movimiento);
            /*
            int saldoFinal = 0;
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            Proyecto Establecimiento = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            int NumeroComprobante = 1;
            if (db.Movimiento.Where(a => a.TipoComprobanteID == 1).Where(a => a.Periodo == periodo).Count() > 0)
            {
                NumeroComprobante = db.Movimiento.Where(a => a.TipoComprobanteID == 1).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
            }
            movimiento.EstablecimientoID = Establecimiento.ID;
            movimiento.CuentaCorrienteID = CuentaCorriente.ID;
            movimiento.Mes = (int)Session["Mes"];
            movimiento.Periodo = (int)Session["Periodo"];
            movimiento.CuentaID = Int32.Parse(Request.Form["CuentaID"].ToString());
            movimiento.NumeroComprobante = NumeroComprobante;
            movimiento.BeneficiarioID = 2;
            movimiento.ComprobanteEgresoID = 1;
            movimiento.TipoComprobanteID = 1;

            /*
            try
            {
                // Si existe registro del saldo actual, se suma monto del ingreso al saldo final
                Saldo Saldo = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID).Where(s => s.Periodo == periodo).Where(s => s.Mes == mes).Single();
                Saldo.SaldoFinal += movimiento.Monto_Ingresos;
                if (ModelState.IsValid)
                {
                    db.Entry(Saldo).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    utils.erroresState(ModelState);
                    return Create(movimiento);
                }
                saldoFinal = Saldo.SaldoFinal;
                utils.Log(1, "Existe registro de saldo para periodo actual, se actualiza el saldo final.");
            }
            catch (Exception e)
            {
                utils.Log(1, "No existe registro de saldo para periodo actual. " + e.InnerException.Message);
                try
                {
                    // Verificamos si existe periodo anterior
                    int periodo_anterior = periodo, mes_anterior = mes;
                    if (mes == 1)
                    {
                        mes_anterior = 12;
                        periodo_anterior--;
                    }
                    else
                    {
                        mes_anterior--;
                    }
                    Saldo SaldoAnterior = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID).Where(s => s.Periodo == periodo_anterior).Where(s => s.Mes == mes_anterior).Single();
                    // Ingresamos nuevo registro de saldo y actualizamos saldo final
                    Saldo SaldoActual = new Saldo();
                    SaldoActual.SaldoInicialCartola = SaldoAnterior.SaldoFinal;
                    SaldoActual.SaldoFinal = SaldoAnterior.SaldoFinal + movimiento.Monto_Ingresos;
                    SaldoActual.Periodo = periodo;
                    SaldoActual.Mes = mes;
                    SaldoActual.CuentaCorrienteID = CuentaCorriente.ID;
                    db.Saldo.Add(SaldoActual);
                    db.SaveChanges();
                    saldoFinal = SaldoActual.SaldoFinal;
                    utils.Log(1, "Existe registro de saldo para periodo anterior y se ingresa y actualiza saldo para periodo actual");
                }
                catch (Exception e2)
                {
                    utils.Log(1, "No existe registro de saldo para periodo anterior. " + e2.Message);
                    try
                    {
                        // Si no existe saldo para periodo actual ni anterior, definimos saldo inicial en CERO en el periodo actual para la cuenta corriente.
                        Saldo SaldoActual = new Saldo();
                        SaldoActual.SaldoInicialCartola = 0;
                        SaldoActual.SaldoFinal = movimiento.Monto_Ingresos;
                        SaldoActual.Periodo = periodo;
                        SaldoActual.Mes = mes;
                        SaldoActual.CuentaCorrienteID = CuentaCorriente.ID;
                        db.Saldo.Add(SaldoActual);
                        db.SaveChanges();
                         
                        saldoFinal = 0;// SaldoActual.SaldoFinal;
                        utils.Log(1, "Se crea registro para saldo para periodo actual y se define saldo inicial CERO.");
                    }
                    catch (Exception e3)
                    {
                        @ViewBag.Mensaje = utils.mensajeError(e3.Message);
                        return Create(movimiento);
                    }
                }
            }
            */
            /*
            movimiento.Saldo = saldoFinal;
            try
            {
                if (ModelState.IsValid)
                {
                    db.Movimiento.Add(movimiento);
                    db.SaveChanges();
                }
                else
                {
                    utils.erroresState(ModelState);
                    return Create(movimiento);
                }
            }
            catch (Exception e)
            {
                @ViewBag.Mensaje = utils.mensajeError(e.Message);
                return Create(movimiento);
            }

            return Create();
             * */
        }
        
        //
        // GET: /Ingresos/Edit/5
 
        public ActionResult Edit(int id)
        {
            Movimiento Ingreso = db.Movimiento.Find(id);
            ViewBag.NroComprobante = Ingreso.NumeroComprobante.ToString();
            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ID_CUENTA_INGRESO"])), Ingreso.CuentaID);
            return View(Ingreso);
        }

        //
        // POST: /Ingresos/Edit/5

        [HttpPost]
        public ActionResult Edit(Movimiento movimiento)
        {
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            int montoOriginal = Int32.Parse(Request.Form["MontoOriginal"].ToString());
            utils.Log(1, "Ingreso a modificar ID: " + movimiento.ID);
            movimiento.CuentaID = Int32.Parse(Request.Form["CuentaID"].ToString());
            if (!movimiento.Periodo.Equals(periodo) || !movimiento.Mes.Equals(mes))
            {
                ViewBag.Mensaje = utils.mensajeError("No es posible modificar este Ingreso ya que pertence a otro período.");
            }
            else
            {
                utils.Log(1, "Modificacion dentro del periodo");
                if (ModelState.IsValid)
                {
                    utils.Log(1, "Ingreso valido");
                    db.Entry(movimiento).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    utils.Log(2, "Ingreso invalido");
                    ViewBag.Mensaje = utils.mensajeError("Ocurrió un error al registrar el ingreso");
                    utils.erroresState(ModelState);
                }

                if (ModelState.IsValid)
                {
                    utils.editarSaldoIngreso(movimiento, montoOriginal);
                }
                else
                {
                    ViewBag.Mensaje = utils.mensajeError("Ocurrió un error al registrar el ingreso");
                    utils.erroresState(ModelState);
                }
            }

            movimiento.Establecimiento = db.Proyecto.Find(movimiento.EstablecimientoID);
            movimiento.CuentaCorriente = db.CuentaCorriente.Find(movimiento.CuentaCorrienteID);
            ViewBag.NroComprobante = movimiento.NumeroComprobante.ToString();
            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["ID_CUENTA_INGRESO"])), movimiento.CuentaID);
            return View(movimiento);
        }

        public ActionResult Anular(int id)
        {
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            Movimiento movimiento = db.Movimiento.Find(id);

            if (movimiento.Periodo != periodo || movimiento.Mes != mes)
            {
                @ViewBag.Mensaje = utils.mensajeOK("No es posible anular este Ingreso ya que pertence a otro período.");
                return Edit(movimiento.ID);
            }

            int monto = movimiento.Monto_Ingresos;

            if (ModelState.IsValid)
            {
                // Anulamos movimiento y asignamos valor UNO.
                movimiento.Nulo = "S";
                db.Entry(movimiento).State = EntityState.Modified;
                movimiento.Monto_Ingresos = 1;
                db.SaveChanges();

                // Actualizamos saldo
                Saldo Saldo = db.Saldo.Where(s => s.CuentaCorrienteID == CuentaCorriente.ID).Where(s => s.Periodo == periodo).Where(s => s.Mes == mes).Single();
                Saldo.SaldoFinal = Saldo.SaldoFinal - monto + 1;
                db.Entry(Saldo).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Create();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}