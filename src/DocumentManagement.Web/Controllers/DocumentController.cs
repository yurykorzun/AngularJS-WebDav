using System.Web.Mvc;
using DocumentManagement.Web.Services;

namespace DocumentManagement.Web.Controllers
{
    public class DocumentController : Controller
    {
        [HttpPost]
        public ActionResult UploadFile(int id)
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                   new DocumentService().UploadFile(file, id);
                }
            }

            return Json(new object{});
        }

        [HttpPost]
        public ActionResult DeleteItem(int id, bool isFolder)
        {
            new DocumentService().DeleteItem(id, isFolder);

            return Json(new object { });
        }

        [HttpPost]
        public ActionResult CreateFolder(int id, string folderName)
        {
            var newFolder = new DocumentService().CreateFolder(id, folderName);

            return Json(newFolder);
        }
    }
}