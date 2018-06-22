using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAG2.Models
{
    public class InventarioBajas
    {
        public int ID { get; set; }
        public int InventarioID { get; set; }
        public int PersonaID { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; }

        public virtual Inventario Item { get; set; }
        public virtual Persona Persona { get; set; }
    }
}