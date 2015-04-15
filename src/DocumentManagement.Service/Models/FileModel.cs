using System;

namespace DocumentManagement.Service.Models
{
    public class FileModel : BaseModel
    {
        public int ParentFolderId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] FileData { get; set; }
        public long? FileDataSize { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public int VersionSeq { get; set; }
    }

}