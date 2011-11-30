using System.Web.Mvc;

namespace Retina.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int? id)
        {
            return View();
        }
    }
}