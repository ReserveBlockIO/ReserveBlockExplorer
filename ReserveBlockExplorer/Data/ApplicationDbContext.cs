using Microsoft.EntityFrameworkCore;
using ReserveBlockExplorer.Models;

namespace ReserveBlockExplorer.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Block> Block { get; set; }
        public DbSet<BlockChainData> BlockChainData { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
    }
}
