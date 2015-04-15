using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.Web.Models;

namespace DocumentManagement.Web.Services
{
    public class DocumentService
    {
        private readonly string _webDavUrl = ConfigurationManager.AppSettings["webDavServer"];

        #region Get

        public DocumentItemModel GetRootFolder()
        {
            var rootFolder = FolderService.GetRootFolder();

            var documentItem = ConvertFolder(rootFolder);

            return documentItem;
        }

        public DocumentItemModel GetFolder(int id)
        {
            var folder = FolderService.GetFolder(id);

            var documentItem = ConvertFolder(folder);

            return documentItem;
        }

        public DocumentItemModel GetItemsForParentId(int parentFolderId)
        {
            var parentFolder = FolderService.GetFolder(parentFolderId);

            var documentItem = ConvertFolder(parentFolder);

            return documentItem;
        }

        #endregion

        #region Create

        public void UploadFile(HttpPostedFileBase file, int parentFolderId)
        {
            using (var documentStream = new MemoryStream())
            {
                file.InputStream.CopyTo(documentStream);
                var bytes = documentStream.ToArray();

                var newFile = new FileModel()
                {
                    ContentType    = file.ContentType,
                    FileData       = bytes,
                    FileDataSize   = file.ContentLength,
                    ParentFolderId = parentFolderId,
                    CreatedUser    = HttpContext.Current.User.Identity.Name,
                    UpdatedUser    = HttpContext.Current.User.Identity.Name,
                    FileName       = file.FileName,
                    VersionSeq     = 0
                };

                newFile = FileService.SaveFile(newFile);
            }
         
        }

        public DocumentItemModel CreateFolder(int parentFolderId, string folderName)
        {
            var newFolder = new FolderModel()
            {
                ParentFolderId = parentFolderId,
                FolderName = folderName,
                CreatedUser = HttpContext.Current.User.Identity.Name,
                UpdatedUser = HttpContext.Current.User.Identity.Name,
                VersionSeq = 0
            };

            newFolder = FolderService.SaveFolder(newFolder);

            var documentItem = ConvertFolder(newFolder);

            return documentItem;
        }

        #endregion

        public void DeleteItem(int id, bool isFolder)
        {
            if (!isFolder)
            {
                FileService.DeleteFile(id);

                return;
            }

            FolderService.DeleteFolder(id);
        }

        #region Private

        private DocumentItemModel ConvertFolder(FolderModel folder)
        {
            var parentUrl = GetFolderUrl(folder);
            var documentItem = new DocumentItemModel
            {
                Id = folder.Id,
                ParentFolderId = folder.ParentFolderId,
                Name = folder.FolderName,
                CreateDate = folder.CreatedDate,
                UpdatedDate = folder.UpdatedDate,
                IsFolder = true,
                ChildItems = GetChildItemsd(folder, parentUrl)
            };

            return documentItem;
        }

        private List<DocumentItemModel> GetChildItemsd(FolderModel parentFolder, string folderUrl)
        {
            var childFolders = FolderService.GetFolderByParentId(parentFolder.Id);
            var childFiles = FileService.GetFileByParentFolder(parentFolder.Id);

            var childItems = childFolders.Select(x => new DocumentItemModel
            {
                Id = x.Id,
                ParentFolderId = parentFolder.ParentFolderId,
                Name = x.FolderName,
                CreateDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                IsFolder = true
            }).ToList();

            childItems.AddRange(childFiles.Select(x => new DocumentItemModel
            {
                Id = x.Id,
                ParentFolderId = parentFolder.ParentFolderId,
                FolderUrl = folderUrl,
                Name = x.FileName,
                CreateDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                IsFolder = false,
                ContentType = x.ContentType
            }).ToList());

            return childItems;
        }

        private string GetFolderUrl(FolderModel folder)
        {
            var parentFolderId = folder.ParentFolderId;
            var urlBuilder = new StringBuilder();

            urlBuilder.Append(folder.FolderName);

            while (parentFolderId.HasValue)
            {
                var parentFolder = FolderService.GetFolder(parentFolderId.Value);
                parentFolderId = parentFolder.ParentFolderId;

                urlBuilder.Insert(0, string.Format("{0}/", parentFolder.FolderName));
            }

            urlBuilder.Insert(0, string.Format("{0}/", _webDavUrl));


            return urlBuilder.ToString();
        }

        #endregion

    }
}