using CustomDataFilter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomDataFilter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetData(string _data)
        {
            //gelen data null ve probel deyilse ise basla!
            //eks halda INdex sehifesine geri don
            if (!String.IsNullOrWhiteSpace(_data))
            {
                // yoxla gorek data hansi fayla yazilacaq
                var filename = _data.CheckData();
                // datanin yazilacagi faylin full path gotur
                var filepath = Path.Combine(Server.MapPath("/Files"), filename);    
                //eger datanin yazilacagi fayl yoxdusa yarat
                if (!System.IO.File.Exists(filepath))
                {

                    FileStream fs = null;
                    try
                    {
                        fs = System.IO.File.Create(filepath);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                //datani fayla yazmaq ucun funksiyani ise sal
                using (StreamWriter w = System.IO.File.AppendText(filepath))
                {
                    Log(_data, w);
                }
            }
            return View("Index");
        }

        [NonAction]
        private void Log(string logMessage, TextWriter w)
        {
            w.WriteLine(logMessage);
            w.WriteLine("-------------------------------");
        }
    }
}