using System;
using System.Diagnostics;
using System.IO;
using SautinSoft;

namespace PDFtoWord.Helper
{
    public static class ConverterService
    {
        // =======================
        // 1) PDF → DOCX
        // =======================
        public static string PdfToDocx(string pdfPath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            string outFile = Path.Combine(
                outputDir,
                Path.GetFileNameWithoutExtension(pdfPath) + ".docx"
            );

            PdfFocus f = new PdfFocus();
            f.OpenPdf(pdfPath);

            if (f.PageCount > 0)
            {
                int result = f.ToWord(outFile);
                if (result != 0)
                    throw new Exception("PDF → Word conversion failed.");
            }

            return outFile;
        }

        // =======================
        // 2) DOCX → PDF (LibreOffice)
        // =======================
        public static string DocxToPdf(string docxPath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            string outFile = Path.Combine(
                outputDir,
                Path.GetFileNameWithoutExtension(docxPath) + ".pdf"
            );

            // LibreOffice must be installed
            string soffice = @"C:\Program Files\LibreOffice\program\soffice.exe";

            if (!File.Exists(soffice))
                throw new Exception("LibreOffice not installed. Cannot convert DOCX → PDF.");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = soffice,
                Arguments = $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{docxPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process p = Process.Start(psi);
            p.WaitForExit();

            if (!File.Exists(outFile))
                throw new Exception("DOCX → PDF conversion failed.");

            return outFile;
        }
    }
}
