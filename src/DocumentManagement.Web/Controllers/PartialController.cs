using System.Web.Mvc;

namespace DocumentManagement.Web.Controllers
{
    public class PartialController : Controller
    {
        [HttpGet]
        public ActionResult Home()
        {
            return PartialView("_Home");
        }

        [HttpGet]
        public ActionResult Folder()
        {
            return PartialView("_Folder");
        }
    }
}