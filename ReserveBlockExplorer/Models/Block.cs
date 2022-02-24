using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReserveBlockExplorer.Models
{
    public class Block
    {
		[Required]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public long Height { get; set; }
		public string ChainRefId { get; set; }
		public long Timestamp { get; set; }
		public string Hash { get; set; }
		public string PrevHash { get; set; }
		public string MerkleRoot { get; set; }
		public string StateRoot { get; set; }
		public decimal TotalAmount { get; set; }
		public string Validator { get; set; }
		public string ValidatorSignature { get; set; }
		public string NextValidators { get; set; }
		public decimal TotalReward { get; set; }
		public int Version { get; set; }
		public int NumOfTx { get; set; }
		public long Size { get; set; }
		public int BCraftTime { get; set; }
		public virtual List<Transaction>? Transactions { get; set; }

	}
}
