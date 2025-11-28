using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WordToPDF.Helper
{
    public class PdfCropService
    {
        public string CropPdf(string inputPdf, string outputPdf, float cropTop, float cropBottom)
        {
            PdfReader reader = new PdfReader(inputPdf);

            using (FileStream fs = new FileStream(outputPdf, FileMode.Create))
            using (PdfStamper stamper = new PdfStamper(reader, fs))
            {
                int totalPages = reader.NumberOfPages;

                for (int i = 1; i <= totalPages; i++)
                {
                    Rectangle original = reader.GetCropBox(i);

                    Rectangle newRect = new Rectangle(
                        original.Left,
                        original.Bottom + cropBottom, // crop bottom
                        original.Right,
                        original.Top - cropTop        // crop top
                    );

                    // Apply the crop box properly
                    stamper.Writer.SetBoxSize("crop", newRect);
                    stamper.Writer.SetBoxSize("trim", newRect);
                    stamper.Writer.SetBoxSize("art", newRect);
                    stamper.Writer.SetBoxSize("bleed", newRect);
                }
            }

            return outputPdf;
        }
    }
}