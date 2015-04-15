using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagement.Data.DataEntities
{
    [Table("File")]
    public class File : BasePoco
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ParentFolderId { get; set; }
        [MaxLength(255)]
        public string FileName { get; set; }
        [MaxLength(255)]
        public string ContentType { get; set; }
        public byte[] FileData { get; set; }
        public long? FileDataSize { get; set; }
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

        [ForeignKey("ParentFolderId")]
        public virtual Folder ParentFolder { get; set; }
    }

}