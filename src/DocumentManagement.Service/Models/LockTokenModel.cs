using System;

namespace DocumentManagement.Service.Models
{
    public class LockTokenModel : BaseModel
    {
        public int LockId { get; set; }
        public string Token { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public int VersionSeq { get; set; }
    }
}