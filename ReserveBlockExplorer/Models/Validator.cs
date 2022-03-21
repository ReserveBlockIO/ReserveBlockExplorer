using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReserveBlockExplorer.Models
{
    public class Validator
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Address { get; set; }
        public string UniqueName { get; set; }
        public long Position { get; set; }
        public decimal Amount { get; set; } //Must be 1000 or more.
        public long EligibleBlockStart { get; set; }
        public string Signature { get; set; }
        public bool IsActive { get; set; }
        public int FailCount { get; set; }
        public string NodeIP { get; set; }
        public string NodeReferenceId { get; set; }
    }
}
