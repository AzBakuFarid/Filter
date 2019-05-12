using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Configuration;
using System.Net;
using System.IO;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace CustomDataFilter.Models
{
    public static class CustomMethods
    {
        //daxil edilen datalari yoxlayaraq serverde hemin datanin yazilacagi faylin adini geri qaytarir
        // bu method HomeController-de cagirilir
        public static string CheckData(this string _data)
        {
            Regex regexAlpha = new Regex(@"^[a-zA-Z]*$");
            Regex regexAlphaNumeric = new Regex(@"^[a-zA-Z0-9]*$");

            //data ancaq herflerdirse alpha.txt faylinin adini qaytar
            if (regexAlpha.IsMatch(_data))
                return ConfigurationManager.AppSettings["alphabetic"];

            //datada herfle beraber reqem de varsa alphanumeric.txt faylinin adini qaytar
            if (regexAlphaNumeric.IsMatch(_data))
                return ConfigurationManager.AppSettings["alphanumeric"];

            //yuxaridaki hallarin hec biri deyilse, demek ki datada elave isareler de var
            //ona gore alphanumericws.txt faylinin adini qaytar
            // diger hal ola bilmez, cunki eger data null ve ya probeldise bu funksiyaya umumiyyetle gelmeyecek
            return ConfigurationManager.AppSettings["alphanumericwithsymbols"];

        }

        // -----------------------------------------------
        // FTP serverle elaqe ucun Timer-i qurasdirib ise salir
        // bu funksiya Global.asax-da cagirilir
        public static void RegisterFtpFileUploading()
        {
            Timer timer = new Timer(15*60000);
            timer.Elapsed += SendFileCallback;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        // -----------------------------------------------
        // FTP servere faylin gonderilmesi isini hazirlayan callback method
        public static void SendFileCallback(object state, ElapsedEventArgs e)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "Files"; //appserverde fayllarin oxunacagi folderin adi
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles(); //appserverde FTP servere gonderilecek fayllari gotur
            // kocurulecek fayl varsa ana task yarat ve hemin taskin icinde fayllarin sayina uygun
            // child tasklar yarat. hemin child tasklarda file FTP servere asinxron gonderilsin ve her biri uzre ugurla basha 
            // catibsa gonderilen fayl appserverden silinsin
            if (Files.Length > 0) {
                Task parentTask = new Task(()=> {
                    var tf = new TaskFactory(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.ExecuteSynchronously);
                    var childTasks = new List<Task>();
                    foreach (ContentWrapper item in prepareContentWrapper(Files))
                    {
                        childTasks.Add(tf.StartNew(()=> { item.UploadToFTPServer();}).
                            ContinueWith((task)=> { File.Delete(item.file.FullName); }, TaskContinuationOptions.OnlyOnRanToCompletion));
                    }
                }) ;
                parentTask.Start();
                parentTask.Wait();
            }
        }

        // -----------------------------------------------
        public static IEnumerable<ContentWrapper> prepareContentWrapper(FileInfo[] _files)
        {
            foreach (var item in _files)
            {
                ContentWrapper cw = new ContentWrapper();
                cw.file = item;
                cw.FtpFoldername = Path.GetFileNameWithoutExtension(item.FullName);
                cw.FtpRequest = (FtpWebRequest)WebRequest.Create($"{ConfigurationManager.AppSettings["ftpserver"]}/{cw.FtpFoldername}/{DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")}.txt");
                cw.FtpRequest.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ftpusername"], ConfigurationManager.AppSettings["ftppassword"]);
                cw.FtpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                yield return cw;
            }
        }

        // -----------------------------------------------
        // fayli FTP servere gonderen method... internetden tapdim... 
        // methodun axirinda faylin ugurla gonderilib gonderilmediyini oyrenmek ucun responce qaytariram
        // hemin cavabdan asli olaraq bu methodun cagirildigi yerde - SendFileCallback() methodunda fayli Appserverden silirem
        public static FtpWebResponse UploadToFTPServer(this ContentWrapper _content)
        {
            byte[] fileContent = null;
            using (StreamReader sr = new StreamReader(_content.file.FullName))
            {
                fileContent = Encoding.UTF8.GetBytes(sr.ReadToEnd());
            }
            using (Stream sw = _content.FtpRequest.GetRequestStream())
            {
                sw.Write(fileContent, 0, fileContent.Length);
            }
            return (FtpWebResponse)_content.FtpRequest.GetResponse();
        }

    }
}

