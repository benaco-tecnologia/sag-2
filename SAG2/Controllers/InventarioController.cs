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
    public class InventarioController : Controller
    {
        private SAG2DB db = new SAG2DB();

        //
        // GET: /Inventario/

        public ViewResult InventarioGeneral()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            try
            {
                ViewBag.Director = db.Rol.Include(r => r.TipoRol).Include(r => r.Persona).Where(r => r.TipoRolID == 1).Where(r => r.ProyectoID == Proyecto.ID).Single().Persona.NombreCompleto;
            }
            catch (Exception)
            {
                ViewBag.Director = "No hay Director definido para este Proyecto.";
            }

            var inventario = db.InventarioItem.Include(i => i.Proyecto).Include(i => i.Egreso).Include(i => i.Especie).Where(i => i.ProyectoID == Proyecto.ID);
            ViewBag.Proyecto = (Proyecto)Session["Proyecto"];
            return View(inventario.ToList());
        }

        /*[HttpGet]
        public ViewResult Alta(int id = 0, int altaID = 0)
        {
            if (altaID > 0)
            {
                ViewBag.Listado = db.Inventario.Where(i => i.AltaID == altaID).OrderBy(i => i.Especie.Nombre).ToList();
                ViewBag.AltaID = altaID;
            }
            else
            {
                ViewBag.AltaID = 0;
            }

            Inventario especie = new Inventario();

            if (id > 0)
            {
                especie = db.Inventario.Find(id);
                ViewBag.AltaID = especie.AltaID;
                ViewBag.ID = id;
                ViewBag.EspecieID = new SelectList(db.Especie.OrderBy(c => c.Nombre), "ID", "Nombre", especie.EspecieID);
                ViewBag.ComprobanteEgreso = especie.Egreso.Egreso.NumeroComprobante;
            }
            else
            {
                ViewBag.ID = 0;
                ViewBag.EspecieID = new SelectList(db.Especie.OrderBy(c => c.Nombre), "ID", "Nombre");
            }

            return View(especie);
        }*/

        [HttpGet]
        public ActionResult AltaEliminar(int id = 0, int movimientoID = 0)
        {
            InventarioItem InventarioItem = db.InventarioItem.Find(id);
            db.InventarioItem.Remove(InventarioItem);
            db.SaveChanges();
            return RedirectToAction("Alta", "Inventario", new { movimientoID = movimientoID, id = 0 });
        }

        [HttpGet]
        public ViewResult Alta(int id = 0, int movimientoID = 0)
        {
            if (movimientoID > 0)
            {
                ViewBag.Listado = db.InventarioAlta.Where(i => i.MovimientoID == movimientoID).OrderBy(i => i.Item.Especie.Nombre).ToList();
                ViewBag.MovimientoID = movimientoID;
            }
            else
            {
                ViewBag.MovimientoID = 0;
            }

            InventarioItem item = new InventarioItem();

            if (id > 0)
            {
                item = db.InventarioItem.Find(id);
                ViewBag.ID = id;
                ViewBag.EspecieID = new SelectList(db.Especie.OrderBy(c => c.Nombre), "ID", "Nombre", item.EspecieID);
                try
                {
                    ViewBag.ComprobanteEgreso = item.Egreso.Egreso.NumeroComprobante;
                }
                catch (Exception)
                {
                    ViewBag.ComprobanteEgreso = "";
                }
                ViewBag.Resolucion = db.InventarioAlta.Where(i => i.MovimientoID == movimientoID && i.ItemID == id).Single().Resolucion;
            }
            else
            {
                ViewBag.ID = 0;
                ViewBag.EspecieID = new SelectList(db.Especie.OrderBy(c => c.Nombre), "ID", "Nombre");
            }

            ViewBag.Proyecto = (Proyecto)Session["Proyecto"];
            return View(item);
        }

        [HttpPost]
        public ActionResult Alta(InventarioItem item)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            Persona Persona = (Persona)Session["Persona"];
            string resolucion = Request.Form["Resolucion"].ToString();
            int movimientoID = Convert.ToInt32(Request.Form["MovimientoID"].ToString());

            item.ProyectoID = Proyecto.ID;
            item.Estado = "P"; 

            InventarioMovimiento movimiento = new InventarioMovimiento();
            InventarioAlta alta = new InventarioAlta();

            // Primer ingreso al inventario
            if (movimientoID == 0)
            {
                movimiento.PersonaID = Persona.ID;
                movimiento.ProyectoID = Proyecto.ID;
                movimiento.FechaCreacion = DateTime.Now;
                movimiento.FechaModificacion = DateTime.Now;
                movimiento.Movimiento = "A";
                db.InventarioMovimiento.Add(movimiento);
                db.SaveChanges();
                movimientoID = movimiento.ID;
            }
            else
            {
                movimiento = db.InventarioMovimiento.Find(movimientoID);
                movimiento.FechaModificacion = DateTime.Now;
                db.Entry(movimiento).State = EntityState.Modified;
                db.SaveChanges();
            }

            // Ingreso nuevo
            if (item.ID == 0)
            {
                db.InventarioItem.Add(item);
                db.SaveChanges();

                alta.MovimientoID = movimientoID;
                alta.ItemID = item.ID;
                alta.Cantidad = item.Cantidad;
                alta.Fecha = DateTime.Now;
                alta.Resolucion = resolucion;

                db.InventarioAlta.Add(alta);
                db.SaveChanges();
            }
            else
            {
                alta = db.InventarioAlta.Where(i => i.MovimientoID == movimientoID && i.ItemID == item.ID).Single();
                alta.Resolucion = resolucion;
                db.Entry(alta).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Alta", "Inventario", new { movimientoID = movimientoID, id = 0 });
        }

        /*
        [HttpPost]
        public ActionResult Alta(Inventario especie)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            Persona Persona = (Persona)Session["Persona"];

            especie.ProyectoID = Proyecto.ID;
            especie.FechaIngreso = DateTime.Now;

            if (especie.ID > 0)
            {
                db.Entry(especie).State = EntityState.Modified;
            }
            else
            {
                // Si no viene AltaID se crea grupo
                if (especie.AltaID == 0)
                {
                    InventarioAltas InventarioAltas = new InventarioAltas();
                    InventarioAltas.ProyectoID = Proyecto.ID;
                    InventarioAltas.PersonaID = Persona.ID;
                    InventarioAltas.Fecha = DateTime.Now;

                    db.InventarioAltas.Add(InventarioAltas);
                    db.SaveChanges();

                    especie.AltaID = InventarioAltas.ID;
                }

                if (ModelState.IsValid)
                {
                    db.Inventario.Add(especie);
                }
                
            }
            
            db.SaveChanges();

            return RedirectToAction("Alta", "Inventario", new { altaID = especie.AltaID, id = 0 });
        }*/

        [HttpGet]
        public ViewResult Baja(int id = 0, int movimientoID = 0)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            if (movimientoID > 0)
            {
                ViewBag.Listado = db.InventarioBaja.Where(i => i.MovimientoID == movimientoID).OrderBy(i => i.Item.Especie.Nombre).ToList();
                ViewBag.MovimientoID = movimientoID;
            }
            else
            {
                ViewBag.MovimientoID = 0;
            }

            InventarioItem item = new InventarioItem();

            if (id > 0)
            {
                item = db.InventarioItem.Find(id);
                ViewBag.ID = id;
                ViewBag.EspecieID = new SelectList(db.Especie.OrderBy(c => c.Nombre), "ID", "Nombre", item.EspecieID);
                try
                {
                    ViewBag.ComprobanteEgreso = item.Egreso.Egreso.NumeroComprobante;
                }
                catch (Exception)
                {
                    ViewBag.ComprobanteEgreso = "";
                }
                ViewBag.Resolucion = db.InventarioBaja.Where(i => i.MovimientoID == movimientoID && i.ItemID == id).Single().Resolucion;
            }
            else
            {
                ViewBag.ID = 0;
                ViewBag.EspecieID = new SelectList(db.Especie.OrderBy(c => c.Nombre), "ID", "Nombre");
            }

            ViewBag.Inventario = db.InventarioItem.Where(i => i.ProyectoID == Proyecto.ID && i.Cantidad > 0).ToList();
            return View(item);
        }

        public ViewResult Index()
        {
            var inventario = db.Inventario.Include(i => i.Proyecto).Include(i => i.Egreso).Include(i => i.Especie);
            return View(inventario.ToList());
        }

        //
        // GET: /Inventario/Details/5

        public ViewResult Details(int id)
        {
            Inventario inventario = db.Inventario.Find(id);
            return View(inventario);
        }

        //
        // GET: /Inventario/Create

        public ActionResult Create()
        {
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "Nombre");
            ViewBag.DetalleEgresoID = new SelectList(db.DetalleEgreso, "ID", "Glosa");
            ViewBag.EspecieID = new SelectList(db.Especie, "ID", "Nombre");
            return View();
        } 

        //
        // POST: /Inventario/Create

        [HttpPost]
        public ActionResult Create(Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                db.Inventario.Add(inventario);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "Nombre", inventario.ProyectoID);
            ViewBag.DetalleEgresoID = new SelectList(db.DetalleEgreso, "ID", "Glosa", inventario.DetalleEgresoID);
            ViewBag.EspecieID = new SelectList(db.Especie, "ID", "Nombre", inventario.EspecieID);
            return View(inventario);
        }
        
        //
        // GET: /Inventario/Edit/5
 
        public ActionResult Edit(int id)
        {
            Inventario inventario = db.Inventario.Find(id);
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "Nombre", inventario.ProyectoID);
            ViewBag.DetalleEgresoID = new SelectList(db.DetalleEgreso, "ID", "Glosa", inventario.DetalleEgresoID);
            ViewBag.EspecieID = new SelectList(db.Especie, "ID", "Nombre", inventario.EspecieID);
            return View(inventario);
        }

        //
        // POST: /Inventario/Edit/5

        [HttpPost]
        public ActionResult Edit(Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProyectoID = new SelectList(db.Proyecto, "ID", "Nombre", inventario.ProyectoID);
            ViewBag.DetalleEgresoID = new SelectList(db.DetalleEgreso, "ID", "Glosa", inventario.DetalleEgresoID);
            ViewBag.EspecieID = new SelectList(db.Especie, "ID", "Nombre", inventario.EspecieID);
            return View(inventario);
        }

        //
        // GET: /Inventario/Delete/5
 
        public ActionResult Delete(int id)
        {
            Inventario inventario = db.Inventario.Find(id);
            return View(inventario);
        }

        //
        // POST: /Inventario/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Inventario inventario = db.Inventario.Find(id);
            db.Inventario.Remove(inventario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        /*
        public ActionResult ListadoAltas()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            return View(db.InventarioAltas.Where(i => i.ProyectoID == Proyecto.ID).OrderByDescending(i => i.ID).ToList());
        }
        */

        public ActionResult ListadoAltas()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            return View(db.InventarioMovimiento.Where(i => i.ProyectoID == Proyecto.ID && i.Movimiento.Equals("A")).OrderByDescending(i => i.ID).ToList());
        }

        public ActionResult ListadoBajas()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            return View(db.InventarioMovimiento.Where(i => i.ProyectoID == Proyecto.ID && i.Movimiento.Equals("B")).OrderByDescending(i => i.ID).ToList());
        }

        public ActionResult ListadoDetalles()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            var egresos = db.DetalleEgreso.Where(m => m.Egreso.ProyectoID == Proyecto.ID).Where(a => a.Temporal == null && a.Nulo == null && a.Cuenta.Codigo.StartsWith("8.1")).OrderByDescending(d => d.ID).ToList();
            return View(egresos);
        }

        public ActionResult Dependencias()
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            return View(db.Dependencia.Where(d => d.ProyectoID == Proyecto.ID).OrderBy(d => d.Nombre).ToList());
        }

        public ActionResult Hoja(int dependenciaID)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            Dependencia Dependencia = db.Dependencia.Find(dependenciaID);
            ViewBag.Dependencia = Dependencia;
            ViewBag.Inventario = db.InventarioItem.Where(i => i.ProyectoID == Proyecto.ID && i.Cantidad > 0).ToList();
            return View(db.InventarioHoja.Where(h => h.DependenciaID == dependenciaID).ToList());
        }

        [HttpPost]
        public ActionResult Hoja(FormCollection form)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            int dependenciaID = Convert.ToInt32(form["dependenciaID"].ToString());
            int itemID = Convert.ToInt32(form["ItemID"].ToString());
            int cantidad = Convert.ToInt32(form["Cantidad"].ToString());
            Dependencia Dependencia = db.Dependencia.Find(dependenciaID);

            InventarioHoja hoja = new InventarioHoja();
            hoja.ItemID = itemID;
            hoja.DependenciaID = dependenciaID;
            hoja.Cantidad = cantidad;
            hoja.Fecha = DateTime.Now;

            db.InventarioHoja.Add(hoja);
            db.SaveChanges();

            return RedirectToAction("Hoja", new { dependenciaID = dependenciaID });
            //ViewBag.Dependencia = Dependencia;
            //ViewBag.Inventario = db.InventarioItem.Where(i => i.ProyectoID == Proyecto.ID && i.Cantidad > 0).ToList();
            //return View(db.InventarioHoja.Where(h => h.DependenciaID == dependenciaID).Select(h => h.Item).ToList());
        }

        public ActionResult EliminarHoja(int id)
        {
            InventarioHoja hoja = db.InventarioHoja.Find(id);
            int dependenciaID = hoja.DependenciaID;

            db.InventarioHoja.Remove(hoja);
            db.SaveChanges();

            return RedirectToAction("Hoja", new { dependenciaID = dependenciaID });
        }
    }
}