using System.Net;
using System.Net.Http;
using System.Web.Http;
using DocumentManagement.Web.Services;

namespace DocumentManagement.Web.Controllers
{
    public class AjaxController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetRootFolder()
        {
            var rootFolder = new DocumentService().GetRootFolder();

            return Request.CreateResponse(HttpStatusCode.OK, rootFolder);
        }

        [HttpGet]
        public HttpResponseMessage GetFolderItems(int id)
        {
            var childItems = new DocumentService().GetItemsForParentId(id);

            return Request.CreateResponse(HttpStatusCode.OK, childItems);
        }

        [HttpGet]
        public HttpResponseMessage GetFolder(int id)
        {
            var folder = new DocumentService().GetFolder(id);

            return Request.CreateResponse(HttpStatusCode.OK, folder);
        }

    }
}