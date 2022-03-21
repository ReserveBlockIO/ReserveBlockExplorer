using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReserveBlockExplorer.Models
{
    public class BanReport
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string BannedAddress { get; set; }
        public string ReporterAddress { get; set; }
        public DateTime ReportDate { get; set; }

    }
}
