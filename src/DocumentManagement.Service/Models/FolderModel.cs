using System;

namespace DocumentManagement.Service.Models
{
    public class FolderModel : BaseModel
    {
        public int? ParentFolderId { get; set; }
        public string FolderName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public int VersionSeq { get; set; }
    }
}