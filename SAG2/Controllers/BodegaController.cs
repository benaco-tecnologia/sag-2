using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;
using SAG2.Classes;

namespace SAG2.Controllers
{
    public class BodegaController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Util utils = new Util();

        public ActionResult Saldos(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0 || Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.periodo = Periodo;
            ViewBag.mes = Mes;
            ViewBag.proyectoID = ((Proyecto)Session["Proyecto"]).ID;

            return View(db.Articulo.OrderBy(a => a.Nombre).ToList());
        }

        public ActionResult Imprimir(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0 || Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.periodo = Periodo;
            ViewBag.mes = Mes;
            ViewBag.proyectoID = ((Proyecto)Session["Proyecto"]).ID;
            ViewBag.NombreProyecto = ((Proyecto)Session["Proyecto"]).NombreLista;

            return View(db.Articulo.OrderBy(a => a.Nombre).ToList());
        }

        public ActionResult Excel(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0 || Mes == 0)
            {
                Periodo = (int)Session["Periodo"];
                Mes = (int)Session["Mes"];
            }

            ViewBag.periodo = Periodo;
            ViewBag.mes = Mes;
            ViewBag.proyectoID = ((Proyecto)Session["Proyecto"]).ID;
            ViewBag.NombreProyecto = ((Proyecto)Session["Proyecto"]).NombreLista;

            return View(db.Articulo.OrderBy(a => a.Nombre).ToList());
        }

    }
}
