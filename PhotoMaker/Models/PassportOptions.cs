using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoMaker.Models
{
    public class PassportOptions
    {
        public string OriginalImageName { get; set; }

        // Later you will use these:
        public int WidthPx { get; set; }
        public int HeightPx { get; set; }
        public string BackgroundColor { get; set; }
        public int SheetCount { get; set; }
        public string SizeType { get; set; }   // "EU", "US", or "Custom"


    }
}