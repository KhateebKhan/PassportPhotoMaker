using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoMaker.Models
{
    public class PhotoResultViewModel
    {
        public string OriginalImageName { get; set; }

        public string OriginalImagePath { get; set; }

        public List<string> ProcessedImages { get; set; }
        public string ProcessedImagePath { get; set; }
        public string BackgroundColor { get; set; }
        public int SheetCount { get; set; }
        public int WidthPx { get; set; }
        public int HeightPx { get; set; }
        public string FinalSheetImagePath { get; set; }
        public int SheetWidthPx { get; set; }
        public string SheetWidthPxpx => SheetWidthPx + "px";


        // later you will add PDF path
        public string PdfPath { get; set; }
    }
}