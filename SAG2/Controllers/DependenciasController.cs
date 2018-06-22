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
    public class DependenciasController : Controller
    {
        private SAG2DB db = new SAG2DB();

        //
        // GET: /Dependencias/

        public ViewResult Index()
        {
            var dependencia = db.Dependencia.Include(d => d.Proyecto);
            return View(dependencia.ToList());
        }

        //
        // GET: /Dependencias/Details/5

        public ViewResult Details(int id)
        {
            Dependencia dependencia = db.Dependencia.Find(id);
            return View(dependencia);
        }

        //
        // GET: /Dependencias/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Dependencias/Create

        [HttpPost]
        public ActionResult Create(Dependencia dependencia)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            dependencia.ProyectoID = Proyecto.ID;
            dependencia.Fecha = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Dependencia.Add(dependencia);
                db.SaveChanges();
                return RedirectToAction("Create");  
            }

            return View(dependencia);
        }
        
        //
        // GET: /Dependencias/Edit/5
 
        public ActionResult Edit(int id)
        {
            Dependencia dependencia = db.Dependencia.Find(id);
            return View(dependencia);
        }

        //
        // POST: /Dependencias/Edit/5

        [HttpPost]
        public ActionResult Edit(Dependencia dependencia)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            dependencia.ProyectoID = Proyecto.ID;
            dependencia.Fecha = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(dependencia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }

            return View(dependencia);
        }

        //
        // GET: /Dependencias/Delete/5
 
        public ActionResult Delete(int id)
        {
            Dependencia dependencia = db.Dependencia.Find(id);
            return View(dependencia);
        }

        //
        // POST: /Dependencias/Delete/5

        [HttpGet, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Dependencia dependencia = db.Dependencia.Find(id);
            db.Dependencia.Remove(dependencia);
            db.SaveChanges();
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}