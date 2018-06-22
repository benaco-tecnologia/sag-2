using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAG2.Models
{
    public class InventarioAlta
    {
        public int ID { get; set; }
        public int MovimientoID { get; set; }
        public int ItemID { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Resolucion { get; set; }

        public virtual InventarioMovimiento Movimiento { get; set; }
        public virtual InventarioItem Item { get; set; }
    }
}