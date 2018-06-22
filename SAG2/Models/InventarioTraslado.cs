using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAG2.Models
{
    public class InventarioTraslado
    {
        public int ID { get; set; }
        public int MovimientoID { get; set; }
        public int ItemOrigenID { get; set; }
        public int ItemDestinoID { get; set; }
        public int ProyectoOrigenID { get; set; }
        public int ProyectoDestinoID { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Resolucion { get; set; }

        public virtual InventarioMovimiento Movimiento { get; set; }
        public virtual InventarioItem ItemOrigen { get; set; }
        public virtual InventarioItem ItemDestino { get; set; }
        public virtual Proyecto ProyectoOrigen { get; set; }
        public virtual Proyecto ProyectoDestino { get; set; }
    }
}