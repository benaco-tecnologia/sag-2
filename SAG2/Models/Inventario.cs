using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAG2.Models
{
    public class Inventario
    {
        public int ID { get; set; }
        public int ProyectoID { get; set; }
        public int DetalleEgresoID { get; set; }
        public int EspecieID { get; set; }
        public int AltaID { get; set; }
        public string Descripcion { get; set; }
        public string Procedencia { get; set; }
        public string Resolucion { get; set; }
        public int Cantidad { get; set; }
        public string Estado { get; set; }
        public int Valor { get; set; }
        public int Año { get; set; }
        public string Observaciones { get; set; }
        public DateTime? FechaIngreso { get; set; }

        public virtual Proyecto Proyecto { get; set; }
        public virtual DetalleEgreso Egreso { get; set; }
        public virtual Especie Especie { get; set; }
        public virtual InventarioAltas Alta { get; set; }
        public virtual List<InventarioBajas> Bajas { get; set; }
    }
}