using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReserveBlockExplorer.Models
{
    public class Transaction
    {

        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(128)]
        public string Hash { get; set; }
        [StringLength(36)]
        public string ToAddress { get; set; }
        [StringLength(36)]
        public string FromAddress { get; set; }
        public decimal Amount { get; set; }
        public long Nonce { get; set; }
        public decimal Fee { get; set; }
        public long Timestamp { get; set; }
        public string? NFTData { get; set; }
        [StringLength(512)]
        public string Signature { get; set; }
        public long Height { get; set; }

        [Required]
        [ForeignKey("Block")]
        public int BlockId { get; set; }
        public virtual Block Block { get; set; }
    }
}
