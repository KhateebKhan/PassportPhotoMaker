using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WordToPDF.Helper;
using WordToPDF.Models;

namespace WordToPDF.Controllers
{
    public class ConvertController : Controller
    {
        PdfWordService service = new PdfWordService();

        // GET: Convert
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(ConvertViewModel model)
        {
            if (model.File == null)
            {
                ViewBag.Error = "Please upload a file.";
                return View();
            }

            string uploadDir = Server.MapPath("~/App_Data/Uploads/");
            string outputDir = Server.MapPath("~/App_Data/Output/");

            Directory.CreateDirectory(uploadDir);
            Directory.CreateDirectory(outputDir);

            string inputPath = Path.Combine(uploadDir,
                Path.GetFileName(model.File.FileName));

            model.File.SaveAs(inputPath);

            string outputFile = "";

            if (model.ConvertType == "PDFtoWord")
                outputFile = service.ConvertPdfToWord(inputPath, outputDir);

            if (model.ConvertType == "WordToPDF")
                outputFile = service.ConvertWordToPdf(inputPath, outputDir);

            model.OutputFilePath = "/App_Data/Output/" + Path.GetFileName(outputFile);

            return View("Result", model);
        }

        public ActionResult Download(string filename)
        {
            string path = Server.MapPath("~/App_Data/Output/" + filename);

            if (!System.IO.File.Exists(path))
                return HttpNotFound("File not found.");

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);

            return File(fileBytes,
                        "application/octet-stream",
                        filename);
        }
        public JsonResult GetProgress()
        {
            int p = ProgressService.GetProgress();
            return Json(new { progress = p }, JsonRequestBehavior.AllowGet);
        }

    }
}