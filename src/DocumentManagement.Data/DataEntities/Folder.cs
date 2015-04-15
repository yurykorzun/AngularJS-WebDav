using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagement.Data.DataEntities
{
    [Table("Folder")]
    public class Folder : BasePoco
    {
        [Key]
        public int Id { get; set; }
        public int? ParentFolderId { get; set; }
        [MaxLength(255)]
        public string FolderName { get; set; }
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