using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;
using SAG2.Classes;

namespace SAG2.Controllers
{ 
    public class ReintegrosController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Util utils = new Util();
        private Constantes ctes = new Constantes();

        //
        // GET: /Reintegros/

        public ViewResult Index()
        {
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            var movimiento = db.Movimiento.Include(m => m.Proyecto).Include(m => m.TipoComprobante).Include(m => m.Cuenta).Include(m => m.Persona).Include(m => m.Proveedor).Include(m => m.CuentaCorriente).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).OrderByDescending(a => a.Periodo).ThenByDescending(a => a.NumeroComprobante);
            return View(movimiento.ToList());
        }

        //
        // GET: /Reintegros/Create

        public ActionResult Create()
        {
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            ViewBag.NroComprobante = "1";
            bool senainfo = false;

            if (senainfo)
            {
                if (db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID != ctes.tipoIngreso).Where(a => a.Periodo == periodo).Count() > 0)
                {   
                    ViewBag.NroComprobante = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID != ctes.tipoIngreso).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
                }
            }
            else
            {
                if (db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Periodo == periodo).Count() > 0)
                {
                    ViewBag.NroComprobante = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
                }
            }
            
            ViewBag.ItemGastoID = new SelectList(db.ItemGasto, "ID", "NombreLista");
            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(ctes.raizCuentaEgresos));
            return View();
        } 

        //
        // POST: /Reintegros/Create

        [HttpPost]
        public ActionResult Create(Movimiento movimiento)
        {
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            movimiento.NumeroComprobante = 1;
            bool senainfo = false;

            if (senainfo)
            {
                if (db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID != ctes.tipoIngreso).Where(a => a.Periodo == periodo).Count() > 0)
                {
                    movimiento.NumeroComprobante = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID != ctes.tipoIngreso).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
                }
            }
            else
            {
                if (db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Periodo == periodo).Count() > 0)
                {
                    movimiento.NumeroComprobante = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante) + 1;
                }
            }

            movimiento.ProyectoID = Proyecto.ID;
            movimiento.CuentaCorrienteID = CuentaCorriente.ID;
            movimiento.Mes = (int)Session["Mes"];
            movimiento.Periodo = (int)Session["Periodo"];
            movimiento.TipoComprobanteID = ctes.tipoReintegro;
            movimiento.Descripcion = movimiento.Descripcion.ToUpper();

            if (movimiento.NDocumento == 0 || movimiento.NDocumento == null)
            {
                movimiento.NDocumento = 1;
            }

            try
            {
                Usuario Usuario = (Usuario)Session["Usuario"];
                movimiento.UsuarioID = Usuario.ID;
                movimiento.FechaCreacion = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.Movimiento.Add(movimiento);
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("Ocurrió un error al registrar el reintegro.");
                }

                if (!utils.ingresarSaldoIngreso(movimiento, ModelState))
                {
                    throw new Exception("Ocurrio un error al actualiza el saldo de la cuenta corriente.");
                }
                else
                {
                    //return RedirectToAction("Create");
                }
            }
            catch (Exception e)
            {
                @ViewBag.Mensaje = utils.mensajeError(e.Message);
            }

            if (Request.Form["ImprimirComprobante"].ToString().Equals("true"))
            {
                return RedirectToAction("Edit", new { @id = movimiento.ID, @imprimir = "true" });
            }
            else
            {
                return RedirectToAction("Create");
            }
        }
        
        //
        // GET: /Reintegros/Edit/5

        public ActionResult Edit(int id, string imprimir = "")
        {
            Movimiento movimiento = db.Movimiento.Find(id);
            int periodo = (int)Session["Periodo"];
            DetalleEgreso detalle = db.DetalleEgreso.Find(movimiento.DetalleEgresoID);
            ViewBag.detalle = detalle;
            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(ctes.raizCuentaEgresos), movimiento.CuentaID);
            ViewBag.Imprimir = imprimir;
            ViewBag.UltimoIdentificador = "0";
            try
            {
                //ViewBag.UltimoIdentificador = db.Movimiento.Where(m => m.ProyectoID == movimiento.ProyectoID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Max(a => a.ID).ToString();
                ViewBag.UltimoIdentificador = db.Movimiento.Include(m => m.Proyecto).Include(m => m.TipoComprobante).Include(m => m.Cuenta).Include(m => m.Persona).Include(m => m.Proveedor).Include(m => m.CuentaCorriente).Where(m => m.TipoComprobanteID == ctes.tipoReintegro).Where(m => m.ProyectoID == movimiento.ProyectoID).Where(a => a.Temporal == null && a.Eliminado == null && a.CuentaID != 1).Max(a => a.ID).ToString();
            }
            catch (Exception)
            {
                ViewBag.UltimoIdentificador = "0";
            }


            return View(movimiento);
        }

        //
        // POST: /Reintegros/Edit/5

        [HttpPost]
        public ActionResult Edit(Movimiento movimiento)
        {
            Usuario Usuario = (Usuario)Session["Usuario"];
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            Persona persona = (Persona)Session["Persona"];
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            CuentaCorriente CuentaCorriente = (CuentaCorriente)Session["CuentaCorriente"];
            int montoOriginal = Int32.Parse(Request.Form["MontoOriginal"].ToString());
            movimiento.CuentaID = Int32.Parse(Request.Form["CuentaID"].ToString());
            //movimiento.DetalleEgresoID = Int32.Parse(Request.Form["DetalleEgresoID"].ToString());
            movimiento.PersonaID = null;
            movimiento.ProveedorID = null;
            movimiento.TipoComprobanteID = ctes.tipoReintegro;
            int originalID = movimiento.ID;
            movimiento.Descripcion = movimiento.Descripcion.ToUpper();
            int? modificadoID = 0;
            ViewBag.UltimoIdentificador = "0";

            try
            {
                ViewBag.UltimoIdentificador = db.Movimiento.Where(m => m.ProyectoID == movimiento.ProyectoID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Max(a => a.ID).ToString();
            }
            catch (Exception)
            { }

            if (!movimiento.Periodo.Equals(periodo) || !movimiento.Mes.Equals(mes))
            {
                if (Usuario.esAdministrador || Usuario.esSupervisor)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            db.Movimiento.Attach(movimiento);
                            db.Entry(movimiento).State = EntityState.Modified;
                            db.SaveChanges();

                            // Se deben actualizar los saldos

                            int mes_comprobante = movimiento.Mes;
                            int periodo_comprobante = movimiento.Periodo;
                            int mes_proyecto = mes;
                            int periodo_proyecto = periodo;

                            utils.RecalcularSaldos(periodo_comprobante, periodo_proyecto, mes_comprobante, mes_proyecto, Proyecto, CuentaCorriente);
                        }
                        catch (Exception e)
                        {
                            utils.Log(2, e.Message);
                            ViewBag.Mensaje = utils.mensajeError("No fue posible modificar este Reintegro, intente nuevamente.");
                        }
                    }
                }
                else
                {
                    // Se elimina cualquier modificación anterior, solo queda la última.
                    try
                    {
                        Autorizacion autorizaciontmp = db.Autorizacion.Where(a => a.OriginalID == originalID).Single();
                        modificadoID = autorizaciontmp.ModificadoID;
                        db.Autorizacion.Remove(autorizaciontmp);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    { }

                    try
                    {
                        Movimiento tmp = db.Movimiento.Find(modificadoID);
                        db.Movimiento.Remove(tmp);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    { }

                    // Edicion del movimiento debe autorizarse, cambios se registran de forma temporal.
                    movimiento.ID = 0;
                    movimiento.Temporal = "S";
                    movimiento.Eliminado = "N";
                    movimiento.UsuarioID = Usuario.ID;
                    db.Movimiento.Add(movimiento);
                    db.SaveChanges();

                    // Se registra en la tabla de Autorizaciones
                    Autorizacion autorizacion = new Autorizacion();
                    autorizacion.OriginalID = originalID;
                    autorizacion.ModificadoID = movimiento.ID;
                    autorizacion.SolicitaID = persona.ID;
                    autorizacion.Tipo = "Modificación";
                    autorizacion.FechaSolicitud = DateTime.Now;

                    db.Autorizacion.Add(autorizacion);
                    db.SaveChanges();

                    ViewBag.Mensaje = utils.mensajeAdvertencia("La modificación ha sido solicitada al Supervisor.");
                }
            }
            else
            {
                movimiento.UsuarioID = Usuario.ID;
                if (movimiento.NDocumento == 0 || movimiento.NDocumento == null)
                {
                    movimiento.NDocumento = 1;
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Entry(movimiento).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Create");
                    }
                    catch (Exception e)
                    {
                        utils.Log(2, e.Message);
                        ViewBag.Mensaje = utils.mensajeError("No fue posible modificar este Ingreso, intente nuevamente");
                    }
                }

                if (utils.actualizarSaldoIngreso(movimiento, ModelState, montoOriginal))
                {
                    @ViewBag.Mensaje = utils.mensajeOK("Ingreso registrado con éxito!");
                }
                else
                {
                    @ViewBag.Mensaje = utils.mensajeError("Ocurrió un error el actualizar los saldos");
                }
            }

            DetalleEgreso detalle = db.DetalleEgreso.Find(movimiento.DetalleEgresoID);
            ViewBag.detalle = detalle;
            ViewBag.Arbol = utils.generarSelectHijos(db.Cuenta.Find(ctes.raizCuentaEgresos), movimiento.CuentaID);
            return View(movimiento);
        }

        public ActionResult Anular(int id)
        {
            int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];
            Movimiento movimiento = db.Movimiento.Find(id);
            Persona persona = (Persona)Session["Persona"];

            if (movimiento.Periodo != periodo || movimiento.Mes != mes)
            {
                movimiento.Temporal = "S";
                movimiento.Nulo = "S";
                movimiento.Eliminado = "N";
                movimiento.Monto_Ingresos = 0;
                db.Movimiento.Add(movimiento);
                db.SaveChanges();

                // Se registra en la tabla de Autorizaciones
                Autorizacion autorizacion = new Autorizacion();
                autorizacion.OriginalID = id;
                autorizacion.ModificadoID = movimiento.ID;
                autorizacion.SolicitaID = persona.ID;
                autorizacion.Tipo = "Anulación";
                autorizacion.FechaSolicitud = DateTime.Now;

                db.Autorizacion.Add(autorizacion);
                db.SaveChanges();

                return RedirectToAction("Edit", new { id = @id, mensaje = "La anulación ha sido solicitada al Supervisor." });
            }

            int monto = movimiento.Monto_Ingresos;

            if (ModelState.IsValid)
            {
                movimiento.Nulo = "S";
                db.Entry(movimiento).State = EntityState.Modified;
                //movimiento.Monto_Ingresos = 0;
                db.SaveChanges();
            }

            if (utils.anularSaldoIngreso(movimiento, ModelState, monto))
            {
                @ViewBag.Mensaje = utils.mensajeOK("Ingreso anulado con éxito!");
            }
            else
            {
                @ViewBag.Mensaje = utils.mensajeError("Ocurrió un error el anular el Ingreso");
            }

            return RedirectToAction("Create");
        }

        public ActionResult ListadoDetalles()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            return View(db.DetalleEgreso.Where(d => d.Egreso.ProyectoID == Proyecto.ID).Where(d => d.Temporal == null && d.Nulo == null).OrderByDescending(d => d.Egreso.Fecha).ThenByDescending(d => d.Egreso.NumeroComprobante).ToList());
        }

        [HttpGet, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            int periodo = (int)Session["Periodo"];
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            Movimiento reintrego = db.Movimiento.Find(id);
            int maxComprobante = db.Movimiento.Where(m => m.ProyectoID == Proyecto.ID).Where(a => a.TipoComprobanteID == ctes.tipoReintegro).Where(a => a.Periodo == periodo).Max(a => a.NumeroComprobante);

            if (reintrego.NumeroComprobante == maxComprobante)
            {
                int monto = reintrego.Monto_Ingresos;

                if (ModelState.IsValid)
                {
                    reintrego.Nulo = "S";
                    db.Entry(reintrego).State = EntityState.Modified;
                    reintrego.Monto_Ingresos = 0;
                    db.SaveChanges();
                }

                utils.anularSaldoIngreso(reintrego, ModelState, monto);
                db.Movimiento.Remove(reintrego);
                db.SaveChanges();
            }

            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}