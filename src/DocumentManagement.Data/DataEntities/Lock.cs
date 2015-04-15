using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagement.Data.DataEntities
{
    [Table("Lock")]
    public class Lock : BasePoco
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int FileId { get; set; }
        [Required]
        public int LockType { get; set; }
        [Required]
        public int ResType { get; set; }
        [Required]
        public int LockScope { get; set; }
        [Required]
        public int LockDepth { get; set; }
        [Required]
        public string LockOwner { get; set; }
        [Required]
        public int LockOwnerType { get; set; }
        [Required]
        public int Timeout { get; set; }
        [Required]
        public DateTime UpdatedDate { get; set; }
        [MaxLength(40)]
        public string UpdatedUser { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [MaxLength(40)]
        public string CreatedUser { get; set; }
        [Required]
        public int VersionSeq { get; set; }

        [ForeignKey("FileId")]
        public virtual File File { get; set; }
    }
}