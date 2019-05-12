using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace CustomDataFilter.Models
{
    public class ContentWrapper
    {
        public FileInfo file { get; set; }
        public string FtpFoldername { get; set; }
        public FtpWebRequest FtpRequest { get; set; }
    }
}