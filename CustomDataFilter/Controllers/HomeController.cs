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
                
                //datanin fayla yazilma ani :)
                using (StreamWriter w = System.IO.File.AppendText(filepath))
                {
                    // data fayla yazilan anda yuz faiz movcud olacaq
                    // amma nezeri olaraq using statementde fayl yarandiqdan sonra Log() method ise dusene qeder faylin 
                    // FTP servere kocurulmesi ile elaqader silinmesi hali ola biler
                    
                    try
                    {
                        Log(_data, w);
                    }
                    catch (FileNotFoundException e)
                    {
                        FileStream fs = System.IO.File.Create(filepath);
                        fs.Close();
                        Log(_data, w);
                        //var mes = e.Message;
                    }
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
