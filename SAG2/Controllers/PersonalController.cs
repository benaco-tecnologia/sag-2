﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;
using SAG2.Classes;
using System.Web.Script.Serialization;

namespace SAG2.Controllers
{ 
    public class PersonalController : Controller
    {
        private SAG2DB db = new SAG2DB();
        private Util utils = new Util();

        //
        // GET: /Personal/

        public ViewResult Index(string q = "")
        {
            Usuario Usuario = (Usuario)Session["Usuario"];
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            var rol = db.Rol.Include(r => r.Persona).Include(r => r.TipoRol).Where(r => r.ProyectoID == Proyecto.ID);
            var persona = from r in rol
                            select r.Persona;
            return View(persona.OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoParterno).ThenBy(p => p.ApellidoMaterno).ToList());
        }

        public ViewResult PopUp()
        {
            Usuario Usuario = (Usuario)Session["Usuario"];
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];
            var rol = db.Rol.Include(r => r.Persona).Include(r => r.TipoRol).Where(r => r.ProyectoID == Proyecto.ID);
            var persona = from r in rol
                          select r.Persona;
            return View(persona.OrderBy(p => p.Nombres).ThenBy(p => p.ApellidoParterno).ThenBy(p => p.ApellidoMaterno).ToList());
        }

        //
        // GET: /Personal/Details/5

        public ViewResult Details(int id)
        {
            Persona persona = db.Persona.Find(id);
            return View(persona);
        }

        //
        // GET: /Personal/IngresarPopUp
        public ActionResult IngresarPopUp()
        {
            ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre");
            return View();
        }

        public ActionResult CerrarPopUp(int id)
        {
            Persona Persona = db.Persona.Find(id);
            return View(Persona);
        }

        [HttpPost]
        public ActionResult IngresarPopUp(Persona persona)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            try
            {
                persona.DV = Request.Form["DVBuscar"].ToString();
            }
            catch (Exception)
            { }

            try
            {
                Persona Persona = db.Persona.Where(p => p.Rut == persona.Rut).Where(p => p.DV == persona.DV).Single();
                Rol rol = new Rol();

                rol.PersonaID = Persona.ID;
                rol.TipoRolID = 9; // Sin Rol
                rol.ProyectoID = Proyecto.ID;
                rol.Comentarios = "Rol asignado automáticamente.";

                try
                {
                    db.Rol.Add(rol);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewBag.Mensaje = utils.mensajeError("Ha ocurrido un error (" + utils.Mensaje(e) + ")");
                    return View(persona);
                }

                return RedirectToAction("CerrarPopUp", new { @id = Persona.ID });
            }
            catch (Exception)
            {
                // Persona no existe y debe crearse
                try
                {
                    if (ModelState.IsValid)
                    {
                        Direccion direccion = persona.Direccion;
                        direccion.Mostrar = 1;
                        direccion.ComunaID = 1;

                        if (Request.Form["ComunaID"] != null && !Request.Form["ComunaID"].ToString().Equals(""))
                        {
                            direccion.ComunaID = Int32.Parse(Request.Form["ComunaID"].ToString());
                        }

                        db.Direccion.Add(direccion);
                        db.SaveChanges();

                        persona.DireccionID = direccion.ID;
                        persona.EspecializacionID = null;
                        persona.FechaNacimiento = null;
                        persona.FechaIngreso = null;
                        persona.Nombres = utils.ToTitle(persona.Nombres);
                        if (persona.ApellidoParterno != null)
                        {
                            persona.ApellidoParterno = utils.ToTitle(persona.ApellidoParterno);
                        }
                        
                        if (persona.ApellidoMaterno != null)
                        {
                            persona.ApellidoMaterno = utils.ToTitle(persona.ApellidoMaterno);
                        }

                        db.Persona.Add(persona);
                        db.SaveChanges();

                        Rol rol = new Rol();
                        rol.PersonaID = persona.ID;
                        rol.TipoRolID = 9; // Sin Rol
                        rol.ProyectoID = Proyecto.ID;
                        rol.Comentarios = "Rol asignado automáticamente.";

                        db.Rol.Add(rol);
                        db.SaveChanges();

                        return RedirectToAction("CerrarPopUp", new { @id = persona.ID });
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Mensaje = utils.mensajeError("Ha ocurrido un error (" + utils.Mensaje(e) + ")");
                }
            }

            ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre", Int32.Parse(Request.Form["RegionID"].ToString()));
            ViewBag.ComunaID = Int32.Parse(Request.Form["ComunaID"].ToString()).ToString();
            return View(persona);
        }

        public ActionResult Create()
        {
            //ViewBag.Mensaje = utils.mensajeAdvertencia("La persona será asignada automáticamente al Proyecto actual.");
            ViewBag.TipoPersonalID = new SelectList(db.TipoPersonal, "ID", "Nombre");
            ViewBag.ProfesionID = new SelectList(db.Profesion.Where(p => p.ID != 1), "ID", "Nombre");
            ViewBag.CargoID = new SelectList(db.Cargo.OrderBy(a => a.Nombre), "ID", "Nombre");
            ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre");
            return View();
        } 

        //
        // POST: /Personal/Create

        [HttpPost]
        public ActionResult Create(Persona persona)
        {
            Proyecto Proyecto = (Proyecto)Session["Proyecto"];

            try
            {
                persona.DV = Request.Form["DVBuscar"].ToString();
            }
            catch (Exception)
            { }

            try
            {
                Persona Persona = db.Persona.Where(p => p.Rut == persona.Rut).Where(p => p.DV == persona.DV).Single();
                Rol rol = new Rol();

                rol.PersonaID = Persona.ID;
                rol.TipoRolID = 9; // Sin Rol
                rol.ProyectoID = Proyecto.ID;
                rol.Comentarios = "Rol asignado automáticamente.";

                try
                {
                    db.Rol.Add(rol);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    ViewBag.Mensaje = utils.mensajeError("La Persona ingresada ya existe en este proyecto, para revisarla presione BUSCAR en el panel izquierdo.");
                    ViewBag.TipoPersonalID = new SelectList(db.TipoPersonal, "ID", "Nombre", persona.TipoPersonalID);
                    ViewBag.ProfesionID = new SelectList(db.Profesion.Where(p => p.ID != 1), "ID", "Nombre", persona.ProfesionID);
                    ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre", Int32.Parse(Request.Form["RegionID"].ToString()));
                    ViewBag.CargoID = new SelectList(db.Cargo.OrderBy(a => a.Nombre), "ID", "Nombre", persona.CargoID);
                    ViewBag.ComunaID = Int32.Parse(Request.Form["ComunaID"].ToString()).ToString();
                    return View(persona);
                }

                return RedirectToAction("Create");
            }
            catch (Exception)
            { 
                // Persona no existe y debe crearse
                try
                {
                    if (ModelState.IsValid)
                    {
                        Direccion direccion = persona.Direccion;
                        direccion.Mostrar = 1;
                        direccion.ComunaID = 1;

                        if (Request.Form["ComunaID"] != null && !Request.Form["ComunaID"].ToString().Equals(""))
                        {
                            direccion.ComunaID = Int32.Parse(Request.Form["ComunaID"].ToString());
                        }
                        
                        db.Direccion.Add(direccion);
                        db.SaveChanges();

                        persona.EspecializacionID = null;
                        persona.FechaNacimiento = null;
                        persona.FechaIngreso = null;

                        if (Request.Form["FechaNacimiento"] != null && !Request.Form["FechaNacimiento"].ToString().Equals(""))
                        {
                            persona.FechaNacimiento = DateTime.Parse(Request.Form["FechaNacimiento"].ToString());
                        }

                        if (Request.Form["FechaIngresoSistema"] != null && !Request.Form["FechaIngresoSistema"].ToString().Equals(""))
                        {
                            persona.FechaIngreso = DateTime.Parse(Request.Form["FechaIngresoSistema"].ToString());
                        }

                        persona.Nombres = utils.ToTitle(persona.Nombres);
                        persona.ApellidoParterno = utils.ToTitle(persona.ApellidoParterno);
                        if (persona.ApellidoMaterno != null)
                        {
                            persona.ApellidoMaterno = utils.ToTitle(persona.ApellidoMaterno);
                        }

                        db.Persona.Add(persona);
                        db.SaveChanges();
                        
                        Rol rol = new Rol();

                        rol.PersonaID = persona.ID;
                        rol.TipoRolID = 9; // Sin Rol
                        rol.ProyectoID = Proyecto.ID;
                        rol.Comentarios = "Rol asignado automáticamente.";

                        db.Rol.Add(rol);
                        db.SaveChanges();

                        return RedirectToAction("Create");
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Mensaje = utils.mensajeError("Ha ocurrido un error (" + e.StackTrace + ")");
                }
            }

            ViewBag.TipoPersonalID = new SelectList(db.TipoPersonal, "ID", "Nombre", persona.TipoPersonalID);
            ViewBag.ProfesionID = new SelectList(db.Profesion.Where(p => p.ID != 1), "ID", "Nombre", persona.ProfesionID);
            ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre", Int32.Parse(Request.Form["RegionID"].ToString()));
            ViewBag.CargoID = new SelectList(db.Cargo.OrderBy(a => a.Nombre), "ID", "Nombre", persona.CargoID);
            ViewBag.ComunaID = Int32.Parse(Request.Form["ComunaID"].ToString()).ToString();
            return View(persona);
        }
        
        //
        // GET: /Personal/Edit/5
 
        public ActionResult Edit(int id)
        {
            if (Session["mensaje"] != null)
            {
                ViewBag.Mensaje = utils.mensajeError(Session["mensaje"].ToString());
                Session.Remove("mensaje");
            }

            Persona persona = db.Persona.Find(id);
            ViewBag.TipoPersonalID = new SelectList(db.TipoPersonal, "ID", "Nombre", persona.TipoPersonalID);
            ViewBag.ProfesionID = new SelectList(db.Profesion.Where(p => p.ID != 1), "ID", "Nombre", persona.ProfesionID);
            ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre", persona.Direccion.Comuna.RegionID);
            ViewBag.CargoID = new SelectList(db.Cargo.OrderBy(a => a.Nombre), "ID", "Nombre", persona.CargoID);
            return View(persona);
        }

        //
        // POST: /Personal/Edit/5

        [HttpPost]
        public ActionResult Edit(Persona persona)
        {
            try
            {
                persona.DV = Request.Form["DVBuscar"].ToString();
            }
            catch (Exception)
            { }

            if (ModelState.IsValid)
            {
                Direccion Direccion = db.Direccion.Find(Int32.Parse(Request.Form["DireccionID"].ToString()));
                Direccion.Calle = persona.Direccion.Calle;
                Direccion.Numero = persona.Direccion.Numero;
                Direccion.Depto = persona.Direccion.Depto;
                Direccion.ComunaID = Int32.Parse(Request.Form["ComunaID"].ToString());
                db.Entry(Direccion).State = EntityState.Modified;
                db.SaveChanges();
                persona.Direccion = Direccion;
                db.Entry(persona).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Create");
            }
            ViewBag.TipoPersonalID = new SelectList(db.TipoPersonal, "ID", "Nombre", persona.TipoPersonalID);
            ViewBag.ProfesionID = new SelectList(db.Profesion.Where(p => p.ID != 1), "ID", "Nombre", persona.ProfesionID);
            ViewBag.RegionID = new SelectList(db.Region.OrderBy(a => a.Nombre), "ID", "Nombre", persona.Direccion.Comuna.RegionID);
            ViewBag.CargoID = new SelectList(db.Cargo.OrderBy(a => a.Nombre), "ID", "Nombre", persona.CargoID);
            return View(persona);
        }

        //
        // GET: /Personal/Delete/5
        /*
        public ActionResult Delete()
        {
            Persona persona = db.Persona.Find(id);
            return View(persona);
        }
        */
        //
        // POST: /Personal/Delete/5

        [HttpGet, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                // Se eliminan todos los roles asociados a la persona
                Proyecto Proyecto = (Proyecto)Session["Proyecto"];
                db.Database.ExecuteSqlCommand("DELETE FROM Rol WHERE PersonaID = " + id + " AND ProyectoID = " + Proyecto.ID);

                // Se elimina la cuenta de usuario
                //db.Database.ExecuteSqlCommand("DELETE FROM Usuario WHERE PersonaID = " + id);

                // Se elimina la persona
                //Persona persona = db.Persona.Find(id);
                //db.Persona.Remove(persona);
                //db.SaveChanges();
                return RedirectToAction("Create");
            } 
            catch (Exception)
            {
                Session.Add("mensaje", "Ocurrió un error al intentar eliminar esta persona, intente nuevamente.");
                return RedirectToAction("Edit", new { id = id });
                //return Edit(id);
            }
        }

        // Obtiene especialidad de profesion
        public string Especialidad(int id)
        {
            var CuentasCorrientes = (from c in db.Especializacion
                                     where c.ProfesionID == id
                                     orderby c.Nombre
                                     select new
                                     {
                                         Value = c.ID,
                                         Text = c.Nombre
                                     }).ToList();

            return new JavaScriptSerializer().Serialize(CuentasCorrientes);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}