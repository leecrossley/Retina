using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Retina.Controllers
{
    public class ImageController : Controller
    {
        [HttpPost]
        public ActionResult ocrImage(string imageData, int x, int y, int w, int h)
        {
            // create image from string
            var imageBytes = Convert.FromBase64String(imageData);
            var ms = new MemoryStream(imageBytes, 0,
                                               imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            var image = Image.FromStream(ms, true);
            // crop
            var cropRect = new Rectangle(x, y, w, h);
            var target = new Bitmap(cropRect.Width, cropRect.Height);
            
            using (var g = Graphics.FromImage(target))
            {
                g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
            // save to temp dir with key
            target.Save(AppDomain.CurrentDomain.BaseDirectory + "/Temp/ocrImage.jpg", ImageFormat.Jpeg);
            // communicate with the ocr

            // get the terms back

            // return

            return View();
        }
    }
}