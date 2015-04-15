using System;

namespace DocumentManagement.Service.Models
{
    public class LockModel : BaseModel
    {
        public int FileId { get; set; }
        public int LockType { get; set; }
        public int ResType { get; set; }
        public int LockScope { get; set; }
        public int LockDepth { get; set; }
        public string LockOwner { get; set; }
        public int LockOwnerType { get; set; }
        public int Timeout { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public int VersionSeq { get; set; }
    }
}