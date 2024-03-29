using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;
using Retina.Properties;

//http://www.webservius.com/cons/subscribe.aspx?p=wisetrend&s=wiseocr

namespace Retina.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult ocrImage(int x, int y, int w, int h)
        {
            var guid = Guid.NewGuid();
            // create image from string
            TextReader trs = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Temp/cancerBase64.txt");
            var file = trs.ReadToEnd();

            file = file.Replace(Environment.NewLine, "");
            file = file.Replace("\f", "");
            var imageBytes = Convert.FromBase64String(file);
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
            target.Save(AppDomain.CurrentDomain.BaseDirectory + "/Temp/" + guid + ".png", ImageFormat.Png);
            
            // communicate with the ocr
            var imageUrl = "http://www.ukfy.co.uk//Temp/" + guid + ".png";
            var key = Settings.Default.ApiKey;
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
            var pathToText = "";
            if (string.IsNullOrEmpty(textURL))
            {
                Console.WriteLine("An error has occurred");
                return Json(new { success = false, result = "" });
            }
            pathToText = wc.DownloadString(textURL);
            Console.WriteLine(pathToText);
            /*
            // get the terms back
            var req = WebRequest.Create(pathToText);
            var resp = req.GetResponse();
            var stream = resp.GetResponseStream();
            var sr = new StreamReader(stream);

            var s = sr.ReadToEnd();
            s = s.Replace(Environment.NewLine, "");*/
            pathToText = pathToText.Replace("\f", "");
            Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Access-Control-Allow-Origin, api-key, Access-Control-Allow-Headers, Accept");
            Response.AddHeader("Access-Control-Allow-Origin", "*");
            return Json(new { success = true, result = pathToText }, JsonRequestBehavior.AllowGet);
        }
    }
}