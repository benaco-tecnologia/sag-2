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
    public class MovimientosBodegaController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Util utils = new Util();
        
        //
        // GET: /MovimientosBodega/

        public ActionResult Index()
        {
            ViewBag.DocumentoID = new SelectList(db.Documento.OrderBy(a => a.ID), "ID", "NombreLista");
            ViewBag.ArticuloID = new SelectList(db.Articulo.OrderBy(a => a.Nombre), "ID", "NombreLista");
            ViewBag.periodo = Session["Periodo"].ToString();
            ViewBag.mes = Session["Mes"].ToString();

            return View();
        }

        public ActionResult ListarMovimientos(int periodo = 0, int mes = 0) 
        {
            if (periodo == 0 || mes == 0)
            { 
                periodo = (int)Session["Periodo"];
                mes = (int)Session["Mes"];
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            return View(db.MovimientoBodega.Where(b => b.Periodo == periodo).Where(b => b.Mes == mes).Where(b => b.ProyectoID == Proyecto.ID).OrderByDescending(b => b.ID).ToList());
        }

        public ActionResult Imprimir(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0 || Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            ViewBag.NombreProyecto = Proyecto.NombreLista;
            ViewBag.Periodo = Periodo;
            ViewBag.Mes = Mes;
            return View(db.MovimientoBodega.Where(b => b.Periodo == Periodo).Where(b => b.Mes == Mes).Where(b => b.ProyectoID == Proyecto.ID).OrderByDescending(b => b.ID).ToList());
        }

        public ActionResult Excel(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0 || Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            ViewBag.NombreProyecto = Proyecto.NombreLista;
            ViewBag.Periodo = Periodo;
            ViewBag.Mes = Mes;
            return View(db.MovimientoBodega.Where(b => b.Periodo == Periodo).Where(b => b.Mes == Mes).Where(b => b.ProyectoID == Proyecto.ID).OrderByDescending(b => b.ID).ToList());
        }

        [HttpPost]
        public string GuardarMovimiento(string tipo_movimiento, int DocumentoID, int? NroDocumento, string Fecha, int ArticuloID, int Cantidad, int PeriodoMes, int PeriodoAnio, string Observaciones, int MovimientosBodegaID = 0)
        {
            try
            {
                MovimientosBodega mb = new MovimientosBodega();
                Proyecto Proyecto = (Proyecto)Session["Proyecto"];
                int periodo = PeriodoAnio;
                int mes = PeriodoMes;

                if (MovimientosBodegaID == 0)
                {
                    mb.Fecha = DateTime.Parse(Fecha);
                    mb.ArticuloID = ArticuloID;
                    mb.Observaciones = Observaciones;
                    mb.Periodo = periodo;
                    mb.Mes = mes;
                    mb.ProyectoID = Proyecto.ID;

                    if (tipo_movimiento.Equals("E"))
                    {
                        mb.DocumentoID = DocumentoID;
                        mb.NroDocumento = (int)NroDocumento;
                        mb.Entrada = Cantidad;
                        mb.Salida = null;
                        saldoBodega(periodo, mes, Proyecto.ID, ArticuloID, Cantidad);
                    }
                    else
                    {
                        // Salida
                        mb.DocumentoID = 1;
                        mb.NroDocumento = 1;
                        mb.Entrada = null;
                        mb.Salida = Cantidad;
                        saldoBodega(periodo, mes, Proyecto.ID, ArticuloID, 0, Cantidad);
                    }

                    db.MovimientoBodega.Add(mb);
                    db.SaveChanges();
                }
                else
                {
                    mb = db.MovimientoBodega.Find(MovimientosBodegaID);
                    int? original= 0;
                    if (periodo != mb.Periodo || mes != mb.Mes)
                    {
                        throw new Exception();
                    }

                    if (tipo_movimiento.Equals("E"))
                    {
                        mb.DocumentoID = DocumentoID;
                        mb.NroDocumento = (int)NroDocumento;
                        original = mb.Entrada;
                        mb.Entrada = Cantidad;
                        mb.Salida = null;
                        modificarSaldoBodega(periodo, mes, mb.ProyectoID, ArticuloID, original, Cantidad);
                    }
                    else
                    {
                        // Salida
                        mb.DocumentoID = 1;
                        mb.NroDocumento = 1;
                        original = mb.Salida;
                        mb.Entrada = null;
                        mb.Salida = Cantidad;
                        modificarSaldoBodega(periodo, mes, mb.ProyectoID, ArticuloID, original, 0, Cantidad);
                    }

                    mb.Fecha = DateTime.Parse(Fecha);
                    mb.ArticuloID = ArticuloID;
                    mb.Observaciones = Observaciones;

                    db.Entry(mb).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return "OK";
            }
            catch
            {
                return "NOK";
            }
        }

        [HttpPost, ActionName("Delete")]
        public string DeleteConfirmed(int mov)
        {
            try
            {
                MovimientosBodega mb = db.MovimientoBodega.Find(mov);
                db.MovimientoBodega.Remove(mb);
                db.SaveChanges();
                return "OK";
            }
            catch (Exception)
            {
                return "NOK";
            }
        }

        public void saldoBodega(int periodo, int mes, int proyectoID, int articuloID, int entrada = 0, int salida = 0)
        {
            try
            {
                Bodega bodega = db.Bodega.Where(b => b.ProyectoID == proyectoID).Where(b => b.ArticuloID == articuloID).OrderByDescending(b => b.Periodo).OrderByDescending(b => b.Mes).Take(1).Single();
                if (bodega.Periodo == periodo && bodega.Mes == mes)
                {
                    // Registro encontrado de periodo actual
                    bodega.Entrada += entrada;
                    bodega.Salida += salida;
                    bodega.Saldo = bodega.Entrada - bodega.Salida;
                    db.Entry(bodega).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    int? saldo = bodega.Saldo;

                    bodega = new Bodega();
                    bodega.ProyectoID = proyectoID;
                    bodega.ArticuloID = articuloID;
                    bodega.Periodo = periodo;
                    bodega.Mes = mes;
                    bodega.Entrada = entrada;
                    bodega.Salida = salida;
                    bodega.Saldo = saldo + entrada - salida;

                    db.Bodega.Add(bodega);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                // No hay registro de bodega para el articulo y proyecto
                Bodega bodega = new Bodega();
                bodega.ProyectoID = proyectoID;
                bodega.ArticuloID = articuloID;
                bodega.Periodo = periodo;
                bodega.Mes = mes;
                bodega.Entrada = entrada;
                bodega.Salida = salida;
                bodega.Saldo = entrada - salida;

                db.Bodega.Add(bodega);
                db.SaveChanges();
            }
        }

        public void modificarSaldoBodega(int periodo, int mes, int proyectoID, int articuloID, int? original, int entrada = 0, int salida = 0)
        {
            try
            {
                Bodega bodega = db.Bodega.Where(b => b.Periodo == periodo).Where(b => b.Mes == mes).Where(b => b.ProyectoID == proyectoID).Where(b => b.ArticuloID == articuloID).Single();
               
                // Registro encontrado de periodo actual
                if (entrada != 0)
                {
                    bodega.Entrada = bodega.Entrada - original + entrada;

                }
                else
                {
                    bodega.Salida = bodega.Salida - original + salida;
                }

                bodega.Saldo = bodega.Entrada - bodega.Salida;
                db.Entry(bodega).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
               
            }
        }
    }
}
