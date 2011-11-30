using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;

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
            var imageUrl = "http://www.rhmg-files.co.uk/image1.png";
            var key = "1hJy2bFmeFaoVpYk-1QD_Gsoe4cIDXYS";
            var xe = new XElement("Job",
                    new XElement("InputURL", imageUrl)
                );
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.ContentType] = "text/xml";
            ServicePointManager.Expect100Continue = false;
            var results = wc.UploadString(
                    "https://svc.webservius.com/v1/wisetrend/wiseocr/submit?wsvKey=" + key,
                    "POST",
                    xe.ToString()
                );
            string textURL = null;
            //Use "dumb polling" every 2 seconds in this simplified example, until done.
            //In a real application, notification with NotifyURL should be used instead.
            while (true)
            {
                var resultsXml = XElement.Parse(results);
                var jobURL = resultsXml.Element("JobURL").Value;
                var status = resultsXml.Element("Status").Value;
                if (status == "Finished")
                {
                    textURL = (
                        from elem in resultsXml.Element("Download").Elements("File")
                        where elem.Element("OutputType").Value == "TXT"
                        select elem.Element("Uri").Value
                       ).First();
                    break;
                }
                //Exit if there's an error
                if ((status != "Submitted") && (status != "Processing")) break;
                Thread.Sleep(2000); // 2 seconds
                results = wc.DownloadString(jobURL);
            }
            if (textURL == null)
            {
                Console.WriteLine("An error has occurred");
            }
            else
            {
                var text = wc.DownloadString(textURL);
                Console.WriteLine(text);
            }
            // get the terms back

            // return

            return View();
        }
    }
}