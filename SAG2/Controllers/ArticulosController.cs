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
    public class ArticulosController : Controller
    {
        private SAG2DB db = new SAG2DB();

        //
        // GET: /Articulos/

        public ViewResult Index()
        {
            var articulo = db.Articulo.Include(a => a.UnidadMedida);
            return View(articulo.ToList());
        }

        //
        // GET: /Articulos/Create

        public ActionResult Create()
        {
            ViewBag.UnidadMedidaID = new SelectList(db.UnidadMedida, "ID", "Descripcion");
            return View();
        } 

        //
        // POST: /Articulos/Create

        [HttpPost]
        public ActionResult Create(Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                db.Articulo.Add(articulo);
                db.SaveChanges();
                return RedirectToAction("Create");  
            }

            ViewBag.UnidadMedidaID = new SelectList(db.UnidadMedida, "ID", "Descripcion", articulo.UnidadMedidaID);
            return View(articulo);
        }
        
        //
        // GET: /Articulos/Edit/5
 
        public ActionResult Edit(int id)
        {
            Articulo articulo = db.Articulo.Find(id);
            ViewBag.UnidadMedidaID = new SelectList(db.UnidadMedida, "ID", "Descripcion", articulo.UnidadMedidaID);
            return View(articulo);
        }

        //
        // POST: /Articulos/Edit/5

        [HttpPost]
        public ActionResult Edit(Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(articulo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.UnidadMedidaID = new SelectList(db.UnidadMedida, "ID", "Descripcion", articulo.UnidadMedidaID);
            return View(articulo);
        }

        //
        // GET: /Articulos/Delete/5
 
        [HttpGet, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Articulo articulo = db.Articulo.Find(id);
            db.Articulo.Remove(articulo);
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