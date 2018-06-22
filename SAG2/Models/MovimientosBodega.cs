using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SAG2.Models
{
    public class MovimientosBodega
    {
        [ScaffoldColumn(false)]
        public int ID { get; set; }
        [Display(Name = "Proyecto")]
        public int ProyectoID { get; set; }
        [Display(Name = "Documento")]
        public int DocumentoID { get; set; }
        [Display(Name = "Artículo")]
        public int ArticuloID { get; set; }
        [Display(Name = "Nº Documento")]
        public int NroDocumento { get; set; }
        [Display(Name = "Período")]
        public int Periodo { get; set; }
        public int Mes { get; set; }
        public DateTime Fecha { get; set; }
        public int? Entrada { get; set; }
        public int? Salida { get; set; }
        public string Observaciones { get; set; }

        public virtual Proyecto Proyecto { get; set; }
        public virtual Articulo Articulo { get; set; }
        public virtual Documento Documento { get; set; }
    }
}