﻿using SAG2.Classes;
using SAG2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SAG2.Controllers
{
    public class SenainfoController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Util utils = new Util();
        private Constantes ctes = new Constantes();
        //
        // GET: /Senainfo/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExportarArchivo(int periodo, int mes)
        {
            /*int periodo = (int)Session["Periodo"];
            int mes = (int)Session["Mes"];*/

            ViewBag.mes = mes;
            ViewBag.periodo = periodo;

            var movimiento = db.Movimiento.Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(a => a.Temporal == null && a.Eliminado == null && ((a.CuentaID != 1 && a.CuentaID != 6) || a.CuentaID == null)).OrderBy(m => m.TipoComprobanteID).ThenBy(m => m.NumeroComprobante);
            return View(movimiento.ToList());
        }

        [HttpPost]
        public ActionResult Exportar(int periodo, int mes)
        {
            ViewBag.mes = mes;
            ViewBag.periodo = periodo;
            ViewBag.Movimientos = db.Movimiento.Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(a => a.Temporal == null && a.Eliminado == null && ((a.CuentaID != 1 && a.CuentaID != 6) || a.CuentaID == null)).OrderBy(m => m.ProyectoID).ThenBy(m => m.TipoComprobanteID).ThenBy(m => m.NumeroComprobante).ToList();

            // Largo del codigo de Sename
            int largoCodigoSename = 7;

            var Proyectos = db.Proyecto.Where(p => p.Cerrado == null && p.Eliminado == null && p.CodSename != null && !p.CodSename.Equals("") && p.CodSename.Length == largoCodigoSename).OrderBy(p => p.ID);
            return View(Proyectos.ToList());
        }

        [HttpPost]
        public ActionResult ExportarOrdenado(int periodo, int mes)
        {
            ViewBag.mes = mes;
            ViewBag.periodo = periodo;
            ViewBag.Movimientos = db.Movimiento.Where(m => m.Periodo == periodo).Where(m => m.Mes == mes).Where(a => a.Temporal == null && a.Eliminado == null && ((a.CuentaID != 1 && a.CuentaID != 6) || a.CuentaID == null)).OrderBy(m => m.Mes).ThenBy(m => m.ProyectoID).ThenBy(m => m.TipoComprobanteID).ThenBy(m => m.NumeroComprobante).ToList();
            //var Proyectos = db.Proyecto.Where(p => p.Cerrado == null && p.Eliminado == null && p.CodSename != null && !p.CodSename.Equals("")).OrderBy(p => p.ID);
            //return View(Proyectos.ToList());
            return View();
        }

    }
}
