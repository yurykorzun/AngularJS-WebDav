using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagement.Data.DataEntities
{
    [Table("LockToken")]
    public class LockToken : BasePoco
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LockId { get; set; }
        [Required]
        public string Token { get; set; }
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

        [ForeignKey("LockId")]
        public virtual Lock Lock { get; set; }
    }
}