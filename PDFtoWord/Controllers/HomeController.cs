using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;


namespace PDFtoWord.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase file, string conversion)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "No file selected." });

                string uploads = Server.MapPath("~/App_Data/Uploads");
                string converted = Server.MapPath("~/App_Data/Converted");

                Directory.CreateDirectory(uploads);
                Directory.CreateDirectory(converted);

                string originalName = Path.GetFileName(file.FileName);
                string srcPath = Path.Combine(uploads, Guid.NewGuid() + "_" + originalName);
                file.SaveAs(srcPath);

                string outputFile = "";

                // =======================
                // PDF → DOCX
                // =======================
                if (conversion == "pdf2docx")
                {
                    if (!originalName.ToLower().EndsWith(".pdf"))
                        return Json(new { success = false, message = "Upload a PDF file." });

                    outputFile = PDFtoWord.Helper.ConverterService.PdfToDocx(srcPath, converted);
                }
                // =======================
                // DOCX → PDF
                // =======================
                else if (conversion == "docx2pdf")
                {
                    if (!originalName.ToLower().EndsWith(".docx"))
                        return Json(new { success = false, message = "Upload a DOCX file." });

                    outputFile = PDFtoWord.Helper.ConverterService.DocxToPdf(srcPath, converted);
                }
                else
                {
                    return Json(new { success = false, message = "Invalid conversion type." });
                }

                return Json(new
                {
                    success = true,
                    message = "Conversion completed!",
                    file = Path.GetFileName(outputFile)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        // Preview uploaded file on a separate page
        public ActionResult Preview(string file)
        {
            if (string.IsNullOrEmpty(file))
                return RedirectToAction("Index");

            // Prevent path traversal
            if (System.IO.Path.GetFileName(file) != file)
                return HttpNotFound();

            string uploads = Server.MapPath("~/App_Data/Uploads");
            string converted = Server.MapPath("~/App_Data/Converted");

            string filePath = Path.Combine(uploads, file);

            if (!System.IO.File.Exists(filePath))
                filePath = Path.Combine(converted, file);

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            ViewBag.FileName = file;
            ViewBag.FileUrl = Url.Content("~/App_Data/Converted/" + file);
            ViewBag.Extension = System.IO.Path.GetExtension(file).ToLowerInvariant();

            return View();

        }

        // Download the uploaded file
        public ActionResult Download(string file)
        {
            if (string.IsNullOrEmpty(file))
                return RedirectToAction("Index");

            // Prevent path injection
            if (Path.GetFileName(file) != file)
                return HttpNotFound();

            string uploads = Server.MapPath("~/App_Data/Uploads");
            string converted = Server.MapPath("~/App_Data/Converted");

            // First check Converted folder (converted output files)
            string filePath = Path.Combine(converted, file);

            if (!System.IO.File.Exists(filePath))
            {
                // If not found, check Uploads folder
                filePath = Path.Combine(uploads, file);
            }

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            string contentType = System.Web.MimeMapping.GetMimeMapping(filePath);
            return File(filePath, contentType, file);
        }


        // Stream file for inline preview (no attachment header)
        public ActionResult Stream(string file)
        {
            if (string.IsNullOrEmpty(file))
                return new HttpStatusCodeResult(400);

            if (System.IO.Path.GetFileName(file) != file)
                return HttpNotFound();

            string uploads = Server.MapPath("~/App_Data/Uploads");
            string converted = Server.MapPath("~/App_Data/Converted");

            string filePath = Path.Combine(uploads, file);

            if (!System.IO.File.Exists(filePath))
                filePath = Path.Combine(converted, file);

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            string contentType = System.Web.MimeMapping.GetMimeMapping(filePath);
            return File(filePath, contentType);

        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}