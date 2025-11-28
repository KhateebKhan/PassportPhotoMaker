using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WordToPDF.Models
{
    public class ConvertViewModel
    {
        public HttpPostedFileBase File { get; set; }
        public string ConvertType { get; set; }
        public string OutputFilePath { get; set; }
    }
}