using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SAG2.Models
{
    public class Articulo
    {
        [ScaffoldColumn(false)]
        public int ID { get; set; }
        [Display(Name = "Unidad de medida")]
        public int UnidadMedidaID { get; set; }
        [Display(Name = "Descripción")]
        public string Nombre { get; set; }

        public virtual string NombreLista {
            get {
                return this.Nombre.ToUpper() + " ("+this.UnidadMedida.Descripcion+")";
            }
        }

        public virtual UnidadMedida UnidadMedida { get; set; }
        public virtual List<Bodega> Bodegas { get; set; }
    }
}