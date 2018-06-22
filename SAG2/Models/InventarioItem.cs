using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAG2.Models
{
    public class InventarioItem
    {
        public int ID { get; set; }
        public int? DetalleEgresoID { get; set; }
        public int ProyectoID { get; set; }
        public int EspecieID { get; set; }
        public string Descripcion { get; set; }
        public string Procedencia { get; set; }
        public int Cantidad { get; set; }
        public int Valor { get; set; }
        public int Periodo { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }

        public virtual DetalleEgreso Egreso { get; set; }
        public virtual Proyecto Proyecto { get; set; }
        public virtual Especie Especie { get; set; }
    }
}