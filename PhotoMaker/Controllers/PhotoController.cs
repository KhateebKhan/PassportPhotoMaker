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

        // ================================================
        // 1️⃣ STEP 1 – Upload
        // ================================================
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase photo, string BackgroundColor, int SheetCount)
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


        // ================================================
        // 2️⃣ STEP 2 – Preview + Options
        // ================================================
        [HttpGet]
        public ActionResult Preview(string img, string bg, int sheets = 1)
        {
            if (String.IsNullOrEmpty(img))
                return RedirectToAction("Upload");

            var model = new PhotoResultViewModel
            {
                OriginalImageName = img,
                OriginalImagePath = "/Content/Output/" + img,
                BackgroundColor = bg,
                SheetCount = sheets
            };

            return View(model);
        }


        // ================================================
        // 3️⃣ STEP 3 – Result → Background Removal → Resize
        // ================================================
        [HttpPost]
        public ActionResult Result(PassportOptions options)
        {
            if (options == null) return RedirectToAction("Upload");

            // 1) Load original file
            string inputPath = Path.Combine(OutputFolder, options.OriginalImageName);
            Bitmap original = new Bitmap(inputPath);

            // 2) Passport dimensions
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

            // 3) CROP face correctly
            Bitmap croppedFace = FaceCropper.CropToPassport(original, passportWidth, passportHeight);

            // -----------------------------------------------------
            // ⭐⭐ 4) AI BACKGROUND REMOVAL (IMPORTANT PART) ⭐⭐
            // -----------------------------------------------------
            Bitmap noBackground = AiBackgroundRemover.RemoveBackground(croppedFace);

            // apply clean background (white / blue / any color)
            Color bgColor = Color.White;
            if (!String.IsNullOrEmpty(options.BackgroundColor))
                bgColor = ColorTranslator.FromHtml(options.BackgroundColor);

            Bitmap finalPassportImage = AiBackgroundRemover.ApplySolidBackground(noBackground, bgColor);

            // -----------------------------------------------------
            // 5) Save the new final cropped+cleaned passport image
            // -----------------------------------------------------
            string croppedFile = "passport_" + Guid.NewGuid() + ".jpg";
            string croppedPath = Path.Combine(OutputFolder, croppedFile);
            finalPassportImage.Save(croppedPath, ImageFormat.Jpeg);

            // use this as input
            inputPath = croppedPath;

            // -----------------------------------------------------
            // 6) Produce copies (do NOT resize)
            // -----------------------------------------------------
            List<string> resizedImages = new List<string>();

            for (int i = 0; i < options.SheetCount; i++)
            {
                string fileName = $"processed_{Guid.NewGuid()}.jpg";
                string savePath = Path.Combine(OutputFolder, fileName);

                System.IO.File.Copy(inputPath, savePath);

                resizedImages.Add("/Content/Output/" + fileName);
            }

            // -----------------------------------------------------
            // 7) Build sheet
            // -----------------------------------------------------
            string sheetFileName = ImageSheetBuilder.BuildSheet(
                resizedImages.ConvertAll(p => Server.MapPath(p)),
                passportWidth,
                passportHeight,
                OutputFolder
            );

            string sheetUrl = "/Content/Output/" + sheetFileName;

            // -----------------------------------------------------
            // 8) View model
            // -----------------------------------------------------
            var vm = new PhotoResultViewModel
            {
                OriginalImagePath = "/Content/Output/" + options.OriginalImageName,
                ProcessedImages = resizedImages,
                SheetCount = options.SheetCount,
                WidthPx = passportWidth,
                HeightPx = passportHeight,
                BackgroundColor = options.BackgroundColor,
                FinalSheetImagePath = sheetUrl
            };

            return View(vm);
        }


    }
}