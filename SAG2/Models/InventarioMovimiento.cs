using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAG2.Models
{
    public class InventarioMovimiento
    {
        public int ID { get; set; }
        public int ProyectoID { get; set; }
        public int PersonaID { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string Movimiento { get; set; }

        public virtual Persona Persona { get; set; }
        public virtual Proyecto Proyeto { get; set; }
    }
}