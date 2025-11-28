using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using GroupDocs.Conversion;
using GroupDocs.Conversion.Options.Convert;

namespace WordToPDF.Helper
{
    public class PdfWordService
    {
        public string ConvertPdfToWord(string inputPath, string outputDir)
        {
            ProgressService.SetProgress(5);

            string output = Path.Combine(outputDir,
                 Path.GetFileNameWithoutExtension(inputPath) + ".docx");

            using (GroupDocs.Conversion.Converter converter =
                    new GroupDocs.Conversion.Converter(inputPath))
            {
                ProgressService.SetProgress(40);

                var options = new WordProcessingConvertOptions();

                ProgressService.SetProgress(70);

                converter.Convert(output, options);

                ProgressService.SetProgress(100);
            }

            return output;
        }


        public string ConvertWordToPdf(string inputPath, string outputDir)
        {
            ProgressService.SetProgress(5);

            // Step 1: Convert Word → PDF
            string output = Path.Combine(outputDir,
                Path.GetFileNameWithoutExtension(inputPath) + ".pdf");

            using (GroupDocs.Conversion.Converter converter =
                    new GroupDocs.Conversion.Converter(inputPath))
            {
                ProgressService.SetProgress(40);

                var options = new PdfConvertOptions();
                ProgressService.SetProgress(70);

                converter.Convert(output, options);

                ProgressService.SetProgress(90);
            }

            // Step 2: Crop top and bottom (strong cropping)
            PdfCropService cropper = new PdfCropService();

            string croppedPdf = Path.Combine(outputDir,
                Path.GetFileNameWithoutExtension(inputPath) + "_cropped.pdf");

            // cropTop: amount to remove from top (pixels)
            // cropBottom: amount to remove from bottom (pixels)

            cropper.CropPdf(output, croppedPdf, cropTop: 150, cropBottom: 50);

            ProgressService.SetProgress(100);

            return croppedPdf;  // return CROPPED file
        }

    }
}