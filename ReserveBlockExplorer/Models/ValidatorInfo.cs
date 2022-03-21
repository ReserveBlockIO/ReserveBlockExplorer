using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReserveBlockExplorer.Models
{
    public class ValidatorInfo
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public bool IsBanned { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastChecked { get; set; }
        public int FailChecks { get; set; }
        public string Address { get; set; }
        public int BlocksIn24Hours { get; set; }
        public int BlocksIn7Days { get; set; }

        [Required]
        [ForeignKey("Validator")]
        public long ValidatorId { get; set; }
        public virtual Validator Validator { get; set; }
    }
}
