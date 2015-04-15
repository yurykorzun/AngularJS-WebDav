using System;
using System.Collections.Generic;

namespace DocumentManagement.Web.Models
{
    public class DocumentItemModel
    {
        public DocumentItemModel()
        {
            ChildItems = new List<DocumentItemModel>();
        }

        public int Id { get; set; }
        public int? ParentFolderId { get; set; }
        public string FolderUrl { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsFolder { get; set; }
        public bool IsLocked { get; set; }

        public List<DocumentItemModel> ChildItems { get; set; }
    }
}