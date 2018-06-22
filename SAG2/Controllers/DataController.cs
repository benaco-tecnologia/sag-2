using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SAG2.Models;

namespace SAG2.Controllers
{
    public class DataController : Controller
    {
        public string SumaDetalleEgreso()
        {
            if (Session["DetalleEgreso"] != null)
            {
                int monto = 0;
                List<DetalleEgreso> lista = (List<DetalleEgreso>)Session["DetalleEgreso"];
                foreach (DetalleEgreso detalle in lista)
                {
                    monto += detalle.Monto;
                }

                return monto.ToString();
            }
            else
            {
                return "0";
            }
        }
    }
}
