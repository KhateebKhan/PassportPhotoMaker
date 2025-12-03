using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            if (file == null || file.ContentLength == 0)
            {
                TempData["Message"] = "Please select a PDF file to upload.";
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = "Please select a file to upload." });
                return RedirectToAction("Index");
            }

            string uploads = Server.MapPath("~/App_Data/Uploads");
            if (!System.IO.Directory.Exists(uploads))
                System.IO.Directory.CreateDirectory(uploads);

            string originalFileName = System.IO.Path.GetFileName(file.FileName);
            string srcPath = System.IO.Path.Combine(uploads, originalFileName);
            file.SaveAs(srcPath);

            // Decide conversion
            if (string.Equals(conversion, "pdf2docx", StringComparison.OrdinalIgnoreCase))
            {
                // Convert PDF -> DOCX
                // Placeholder: no conversion library included. Return uploaded PDF for now.
                // To implement: use a library (e.g., Syncfusion, Aspose, Spire.PDF) or call LibreOffice headless.
                TempData["Message"] = "Uploaded PDF saved. Conversion to DOCX is not yet implemented.";
                if (Request.IsAjaxRequest())
                    return Json(new { success = true, message = TempData["Message"], file = originalFileName });
                return RedirectToAction("Index");
            }
            else if (string.Equals(conversion, "docx2pdf", StringComparison.OrdinalIgnoreCase))
            {
                // Convert DOCX -> PDF
                // Validate input
                if (!originalFileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Message"] = "Please upload a .docx file for Word → PDF conversion.";
                    if (Request.IsAjaxRequest())
                        return Json(new { success = false, message = TempData["Message"] });
                    return RedirectToAction("Index");
                }

                // Placeholder: not implemented
                TempData["Message"] = "Uploaded DOCX saved. Conversion to PDF is not yet implemented.";
                if (Request.IsAjaxRequest())
                    return Json(new { success = true, message = TempData["Message"], file = originalFileName });
                return RedirectToAction("Index");
            }

            TempData["Message"] = "File uploaded successfully: " + originalFileName;
            if (Request.IsAjaxRequest())
                return Json(new { success = true, message = TempData["Message"], file = originalFileName });
            return RedirectToAction("Index");
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
            string filePath = System.IO.Path.Combine(uploads, file);
            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            ViewBag.FileName = file;
            ViewBag.FileUrl = Url.Content("~/App_Data/Uploads/" + file);
            ViewBag.Extension = System.IO.Path.GetExtension(file).ToLowerInvariant();
            return View();
        }

        // Download the uploaded file
        public ActionResult Download(string file)
        {
            if (string.IsNullOrEmpty(file))
                return RedirectToAction("Index");

            if (System.IO.Path.GetFileName(file) != file)
                return HttpNotFound();

            string uploads = Server.MapPath("~/App_Data/Uploads");
            string filePath = System.IO.Path.Combine(uploads, file);
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
            string filePath = System.IO.Path.Combine(uploads, file);
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