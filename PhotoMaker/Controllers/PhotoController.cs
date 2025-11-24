using PhotoMaker.Helper;
using PhotoMaker.Helpers;
using PhotoMaker.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace YourProject.Controllers
{
    public class PhotoController : Controller
    {
        private string OutputFolder
        {
            get
            {
                string path = Server.MapPath("~/Content/Output/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }


        // ============================================
        // STEP 1 – UPLOAD
        // ============================================
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Upload(
    HttpPostedFileBase photo,
    string BackgroundColor = "white",
    int SheetCount = 6)
        {
            if (photo == null || photo.ContentLength == 0)
            {
                TempData["Error"] = "Please upload a valid image file.";
                return RedirectToAction("Upload");
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            string ext = Path.GetExtension(photo.FileName).ToLower();

            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Only JPG and PNG images are allowed.";
                return RedirectToAction("Upload");
            }

            string fileName = "original_" + Guid.NewGuid() + ext;
            string savePath = Path.Combine(OutputFolder, fileName);
            photo.SaveAs(savePath);

            return RedirectToAction("Preview", new
            {
                img = fileName,
                bg = BackgroundColor,
                sheets = SheetCount
            });
        }


        // ============================================
        // STEP 2 – PREVIEW PAGE
        // ============================================
        [HttpGet]
        public ActionResult Preview(string img, string bg, int sheets = 1)
        {
            if (string.IsNullOrEmpty(img))
                return RedirectToAction("Upload");

            var model = new PhotoResultViewModel
            {
                OriginalImageName = img,
                OriginalImagePath = "/Content/Output/" + img,
                BackgroundColor = bg,
                SheetCount = sheets,
            };

            return View(model);
        }




        // ============================================
        // STEP 3 – FINAL PROCESSING (POST)
        // ============================================
        [HttpPost]
        public ActionResult Result(PassportOptions options)
        {
            if (options == null)
                return RedirectToAction("Upload");

            // ---------------------------------------------
            // 1) Load original uploaded file
            // ---------------------------------------------
            string inputPath = Path.Combine(OutputFolder, options.OriginalImageName);
            if (!System.IO.File.Exists(inputPath))
                return RedirectToAction("Upload");

            Bitmap original = new Bitmap(inputPath);

            // ---------------------------------------------
            // 2) Determine passport size
            // ---------------------------------------------
            int passportWidth = options.WidthPx;
            int passportHeight = options.HeightPx;

            if (options.SizeType == "EU")
            {
                passportWidth = 413;
                passportHeight = 531;
            }
            else if (options.SizeType == "US")
            {
                passportWidth = 600;
                passportHeight = 600;
            }

            // ---------------------------------------------
            // 3) Face cropper
            // ---------------------------------------------
            Bitmap croppedFace = FaceCropper.CropToPassport(original, passportWidth, passportHeight);

            // ---------------------------------------------
            // 4) AI Background removal
            // ---------------------------------------------
            Bitmap noBg = AiBackgroundRemover.RemoveBackground(croppedFace);

            // Apply background color
            Color bgColor = Color.White;
            if (!string.IsNullOrEmpty(options.BackgroundColor))
                bgColor = ColorTranslator.FromHtml(options.BackgroundColor);

            Bitmap finalPassportImage = AiBackgroundRemover.ApplySolidBackground(noBg, bgColor);

            // Save final cleaned image temporarily
            string passportName = "passport_" + Guid.NewGuid() + ".jpg";
            string passportPath = Path.Combine(OutputFolder, passportName);
            finalPassportImage.Save(passportPath, ImageFormat.Jpeg);


            // ---------------------------------------------
            // 5) Generate multiple copies (for preview)
            // ---------------------------------------------
            List<string> processedImages = new List<string>();
            for (int i = 0; i < options.SheetCount; i++)
            {
                string fileName = $"processed_{Guid.NewGuid()}.jpg";
                string savePath = Path.Combine(OutputFolder, fileName);

                System.IO.File.Copy(passportPath, savePath);

                processedImages.Add("/Content/Output/" + fileName);
            }


            // ---------------------------------------------
            // 6) Build FINAL SHEET in selected PAPER SIZE
            // ---------------------------------------------
            int sheetW = options.PaperWidth > 0 ? options.PaperWidth : 2480; // A4 default
            int sheetH = options.PaperHeight > 0 ? options.PaperHeight : 3508;

            Bitmap finalSheet = PassportSheetBuilder.BuildCustomSheet(
                finalPassportImage,
                options.SheetCount,
                sheetW,
                sheetH
            );

            string sheetFile = "sheet_" + Guid.NewGuid() + ".jpg";
            string sheetPath = Path.Combine(OutputFolder, sheetFile);
            finalSheet.Save(sheetPath, ImageFormat.Jpeg);


            // ---------------------------------------------
            // 7) ViewModel
            // ---------------------------------------------
            var vm = new PhotoResultViewModel
            {
                OriginalImagePath = "/Content/Output/" + options.OriginalImageName,
                ProcessedImages = processedImages,
                SheetCount = options.SheetCount,
                WidthPx = passportWidth,
                HeightPx = passportHeight,
                BackgroundColor = options.BackgroundColor,
                FinalSheetImagePath = "/Content/Output/" + sheetFile,

                // NEW
                PaperWidthPx = sheetW,
                PaperHeightPx = sheetH
            };

            return View(vm);
        }
    }
}
