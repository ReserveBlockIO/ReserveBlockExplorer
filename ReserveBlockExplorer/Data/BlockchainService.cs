using Microsoft.EntityFrameworkCore;
using ReserveBlockExplorer.Models;

namespace ReserveBlockExplorer.Data
{
    public class BlockchainService
    {
        #region Private Variables
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlockchainService> _logger;

        public BlockchainService(ApplicationDbContext context,
            ILogger<BlockchainService> logger)
        {
            _context = context;
            _logger = logger;
        }
        #endregion

        public async Task<List<Block>> GetRecentBlocks()
        {
            var blockChain = _context.BlockChainData.FirstOrDefault();
            var blocksAbove = blockChain.Height != 0 ? blockChain.Height - 10  : 0;

            var blocks = await _context.Block.Where(x => x.Height > blocksAbove)
                .AsNoTracking()
                .ToListAsync();

            return blocks;
        }

        public async Task<ValidatorInfo?> GetAddressInfo(string addr)
        {
            var valInfo = await _context.ValidatorInfo.Where(x => x.Address == addr)
                .AsNoTracking()
                .Include(x => x.Validator)
                .FirstOrDefaultAsync();

            if(valInfo != null)
            {
                return valInfo;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Transaction>> GetRecentTransactions()
        {
            var blockChain = _context.BlockChainData.FirstOrDefault();
            var blocksAbove = blockChain.Height - 10;

            var transactions = await _context.Transaction.Where(x => x.Height > blocksAbove)
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }

        public async Task<List<IGrouping<string, Block>>> GetMasternodes()
        {
            var blockChain = _context.BlockChainData.FirstOrDefault();
            var blocksAbove = blockChain.Height != 0 ? blockChain.Height - 1000 : 0;

            var blocks = await _context.Block.Where(x => x.Height > blocksAbove)
                .AsNoTracking()
                .ToListAsync();

            var blocksGrouped = blocks.GroupBy(x => x.Validator).ToList();

            return blocksGrouped;
        }

        public async Task<Block> GetBlock(string id)
        {
            var blockHeight = Convert.ToInt64(id);
            var block =  await _context.Block.Where(x => x.Height == blockHeight)
                .Include(x => x.Transactions)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return block;
        }

        public async Task<List<Transaction>> GetBlockTransactions(string id)
        {
            var blockHeight = Convert.ToInt64(id);
            var trxs = await _context.Transaction.Where(x => x.Height == blockHeight)
                .AsNoTracking()
                .ToListAsync();

            return trxs;
        }

        public async Task<Transaction> GetTransaction(string id)
        {
            var txhash = (id);
            var tx = await _context.Transaction.Where(x => x.Hash == txhash)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return tx;
        }

        public async Task<string> GetSearch(string id)
        {  
            if(id.Length < 14)
            {
                var idCheck = id.All(Char.IsDigit);
                if(idCheck == true)
                {
                    var height = Convert.ToInt64(id);
                    var block = _context.Block.Where(x => x.Height == height)
                    .Include(x => x.Transactions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                    if (block == null)
                        return ("na");
                    return ("blk");
                }
            }
            else
            {
                var txhash = id;
                var tx = _context.Transaction.Where(x => x.Hash == txhash)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (tx == null)
                    return "na";

                return "tx";
            }

            return ("blk");
        }

    }
}
