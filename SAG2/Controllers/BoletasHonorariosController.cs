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
    public class BoletasHonorariosController : Controller
    {
        private SAG2DB db = new SAG2DB();

        //
        // GET: /BoletasHonorarios/

        public ViewResult Index()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            //int Periodo = (int)Session["Periodo"];
            //int Mes = (int)Session["Mes"];

            var boletahonorario = db.BoletaHonorario.Include(b => b.Persona).Include(b => b.Proyecto).Where(b => b.ProyectoID == Proyecto.ID).OrderByDescending(b => b.Fecha);
            return View(boletahonorario.ToList());
        }

        //
        // GET: /BoletasHonorarios/Details/5

        public ViewResult Details(int id)
        {
            BoletaHonorario boletahonorario = db.BoletaHonorario.Find(id);
            return View(boletahonorario);
        }

        //
        // GET: /BoletasHonorarios/Create

        public ActionResult IngresarPopUp(int personalID = 0, int proveedorID = 0, string rut = "", string dv = "", string beneficiario = "")
        {
            if (personalID != 0) 
            {
                @ViewBag.PersonalID = personalID.ToString();
                @ViewBag.NombreLista = db.Persona.Find(personalID).NombreLista;
            }
            else if (proveedorID != 0) 
            {
                @ViewBag.ProveedorID = proveedorID.ToString();
                @ViewBag.NombreLista = db.Proveedor.Find(proveedorID).NombreLista;
            }
            else 
            {
                @ViewBag.Rut = rut.ToString();
                @ViewBag.DV = dv.ToString();
                @ViewBag.Beneficiario = beneficiario.ToString();
                @ViewBag.NombreLista = rut + "-" + dv + " " + beneficiario;
            }

            @ViewBag.Title = "Ingresar Boleta de Honorarios";
            return View();
        }

        [HttpPost]
        public ActionResult IngresarPopUp(BoletaHonorario boletahonorario)
        {
            if (!Request.Form["PersonaID"].ToString().Equals(""))
            {
                boletahonorario.ProveedorID = null;
                boletahonorario.Rut = null;
                boletahonorario.DV = null;
                @ViewBag.PersonalID = boletahonorario.PersonaID.ToString();
                @ViewBag.NombreLista = db.Persona.Find(boletahonorario.PersonaID).NombreLista;
            }
            else if (!Request.Form["ProveedorID"].ToString().Equals(""))
            {
                boletahonorario.PersonaID = null;
                boletahonorario.Rut = null;
                boletahonorario.DV = null;
                @ViewBag.ProveedorID = boletahonorario.ProveedorID.ToString();
                @ViewBag.NombreLista = db.Proveedor.Find(boletahonorario.ProveedorID).NombreLista;
            }
            else if (!Request.Form["Rut"].ToString().Equals(""))
            {
                boletahonorario.ProveedorID = null;
                boletahonorario.PersonaID = null;
                @ViewBag.Rut = boletahonorario.Rut.ToString();
                @ViewBag.DV = boletahonorario.DV.ToString();
                @ViewBag.Beneficiario = boletahonorario.Beneficiario.ToString();
                @ViewBag.NombreLista = boletahonorario.Rut + "-" + boletahonorario.DV + " " + boletahonorario.Beneficiario;
            }
            else
            {
                throw new Exception("El beneficiario seleccionado no es válido.");
            }

            @ViewBag.Title = "Ingresar Boleta de Honorarios";
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            boletahonorario.Periodo = (int)Session["Periodo"];
            boletahonorario.Mes = (int)Session["Mes"];
            boletahonorario.ProyectoID = Proyecto.ID;
            boletahonorario.EgresoID = null;
            boletahonorario.Electronica = null;
            boletahonorario.Nula = null;

            if (Request.Form["Electronica"] != null)
                boletahonorario.Electronica = "S";

            if (Request.Form["Nula"] != null)
                boletahonorario.Nula = "S";
            
            if (ModelState.IsValid)
            {
                db.BoletaHonorario.Add(boletahonorario);
                db.SaveChanges();
                return RedirectToAction("CerrarPopUp", new { @id = boletahonorario.ID });
            }

            return RedirectToAction("IngresarPopUp", new { @personalID = boletahonorario.PersonaID });
        }

        public ActionResult CerrarPopUp(int id)
        {
            BoletaHonorario boletahonorario = db.BoletaHonorario.Find(id);
            return View(boletahonorario);
        }

        public ActionResult Create()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            var rol = db.Rol.Include(r => r.Persona).Include(r => r.TipoRol).Where(r => r.ProyectoID == Proyecto.ID);
            var persona = from r in rol
                          select r.Persona;
            ViewBag.PersonaID = new SelectList(persona, "ID", "NombreLista");
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "NombreLista");
            return View();
        } 

        //
        // POST: /BoletasHonorarios/Create

        [HttpPost]
        public ActionResult Create(BoletaHonorario boletahonorario)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            boletahonorario.Periodo = (int)Session["Periodo"];
            boletahonorario.Mes = (int)Session["Mes"];
            boletahonorario.ProyectoID = Proyecto.ID;
            boletahonorario.EgresoID = null;
            boletahonorario.Electronica = null;
            boletahonorario.Nula = null;

            if (Request.Form["Electronica"] != null)
                boletahonorario.Electronica = "S";

            if (Request.Form["Nula"] != null)
                boletahonorario.Nula = "S";

            if (ModelState.IsValid)
            {
                db.BoletaHonorario.Add(boletahonorario);
                db.SaveChanges();
                return RedirectToAction("Create");  
            }

            var rol = db.Rol.Include(r => r.Persona).Include(r => r.TipoRol).Where(r => r.ProyectoID == Proyecto.ID);
            var persona = from r in rol
                          select r.Persona;
            ViewBag.PersonaID = new SelectList(persona, "ID", "NombreLista", boletahonorario.PersonaID);
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "NombreLista", boletahonorario.ProyectoID);
            return View(boletahonorario);
        }
        
        //
        // GET: /BoletasHonorarios/Edit/5
 
        public ActionResult Edit(int id)
        {
            BoletaHonorario boletahonorario = db.BoletaHonorario.Find(id);
            var rol = db.Rol.Include(r => r.Persona).Include(r => r.TipoRol).Where(r => r.ProyectoID == boletahonorario.ProyectoID);
            var persona = from r in rol
                          select r.Persona;
            ViewBag.PersonaID = new SelectList(persona, "ID", "NombreLista", boletahonorario.PersonaID);
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "NombreLista", boletahonorario.ProyectoID);
            return View(boletahonorario);
        }

        //
        // POST: /BoletasHonorarios/Edit/5

        [HttpPost]
        public ActionResult Edit(BoletaHonorario boletahonorario)
        {
            boletahonorario.EgresoID = null;
            boletahonorario.Electronica = null;
            boletahonorario.Nula = null;

            if (Request.Form["Electronica"] != null)
                boletahonorario.Electronica = "S";

            if (Request.Form["Nula"] != null)
                boletahonorario.Nula = "S";

            if (ModelState.IsValid)
            {
                db.Entry(boletahonorario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            var rol = db.Rol.Include(r => r.Persona).Include(r => r.TipoRol).Where(r => r.ProyectoID == boletahonorario.ProyectoID);
            var persona = from r in rol
                          select r.Persona;
            ViewBag.PersonaID = new SelectList(persona, "ID", "NombreLista", boletahonorario.PersonaID);
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "NombreLista", boletahonorario.ProyectoID);
            return View(boletahonorario);
        }

        public ActionResult ListadoEgreso(int personalID = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            if (personalID != 0)
            {
                return View(db.BoletaHonorario.Where(d => d.PersonaID == personalID && d.EgresoID == null && d.ProyectoID == Proyecto.ID).Where(d => d.Nula == null).OrderByDescending(b => b.Fecha).ToList());
            }
            else
            {
                return View(db.BoletaHonorario.Where(d => d.EgresoID == null && d.Nula == null && d.ProyectoID == Proyecto.ID).OrderByDescending(b => b.Fecha).ToList());
            }
        }

        //
        // GET: /BoletasHonorarios/Delete/5
 
        [HttpGet, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            BoletaHonorario boletahonorario = db.BoletaHonorario.Find(id);
            db.BoletaHonorario.Remove(boletahonorario);
            db.SaveChanges();
            return RedirectToAction("Create");
        }

        public ActionResult Reporte(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0)
            {
                ViewBag.Periodo = (int)Session["Periodo"];
                ViewBag.Mes = (int)Session["Mes"];
            }
            else
            {
                ViewBag.Periodo = Periodo;
                ViewBag.Mes = Mes;
            }

            var proyecto = db.Proyecto.OrderBy(p => p.CodCodeni).Where(r => r.Eliminado == null).Where(r => r.Cerrado == null).ToList();
            return View(proyecto);
        }

        public ActionResult ReporteImprimir(int Periodo = 0, int Mes = 0)
        {
            if (Periodo == 0)
            {
                ViewBag.Periodo = (int)Session["Periodo"];
                ViewBag.Mes = (int)Session["Mes"];
            }
            else
            {
                ViewBag.Periodo = Periodo;
                ViewBag.Mes = Mes;
            }

            var proyecto = db.Proyecto.OrderBy(p => p.CodCodeni).Where(r => r.Eliminado == null).Where(r => r.Cerrado == null).ToList();
            return View(proyecto);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}