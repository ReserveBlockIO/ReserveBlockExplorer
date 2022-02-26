using Newtonsoft.Json;
using ReserveBlockExplorer.Data;
using ReserveBlockExplorer.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ReserveBlockExplorer
{
    public class TaskScheduler : IHostedService, IDisposable
    {
        private readonly ILogger<TaskScheduler> logger;
        private Timer timer;
        private Timer inactiveNodeTimer;
        private IHostEnvironment _hostEnv;
        private readonly IServiceScopeFactory scopeFactory;
        private string walletAPIURL = "https://localhost:7777";

        public TaskScheduler(ILogger<TaskScheduler> logger, IHostEnvironment hostEnv, IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            _hostEnv = hostEnv;
            this.scopeFactory = scopeFactory;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(o =>
            {

                SyncChain();

            },
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(25));


            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Error");

            return Task.CompletedTask;
        }

        public async void SyncChain()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var chainData = _context.BlockChainData.FirstOrDefault();

                if(chainData == null)
                {
                    BlockChainData bcd = new BlockChainData {
                        Height = 0,
                        RBXMinted = 0,
                        TotalRBX = 372000000
                    };

                    _context.BlockChainData.Add(bcd);
                    await _context.SaveChangesAsync();

                    chainData = _context.BlockChainData.FirstOrDefault();
                }
                try
                {
                    HttpClientHandler clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                    using (HttpClient client = new HttpClient(clientHandler))
                    {
                        string endpoint = walletAPIURL + "/api/V1/SendBlock/" + chainData.Height;

                        using (var Response = await client.GetAsync(endpoint))
                        {
                            if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                string data = await Response.Content.ReadAsStringAsync();

                                if (data != "NNB")
                                {
                                    var block = JsonConvert.DeserializeObject<BlockFromWallet>(data);

                                    var blockCheck = _context.Block.Where(x => x.Height == block.Height).FirstOrDefault();

                                    if (blockCheck == null)
                                    {
                                        var transactions = block.Transactions.ToList();
                                        Block nBlock = new Block
                                        {
                                            BCraftTime = block.BCraftTime,
                                            ChainRefId = block.ChainRefId,
                                            Hash = block.Hash,
                                            Height = block.Height,
                                            MerkleRoot = block.MerkleRoot,
                                            NextValidators = block.NextValidators,
                                            NumOfTx = block.NumOfTx,
                                            PrevHash = block.PrevHash,
                                            Size = block.Size,
                                            StateRoot = block.StateRoot,
                                            Timestamp = block.Timestamp,
                                            TotalAmount = block.TotalAmount,
                                            TotalReward = block.TotalReward,
                                            Validator = block.Validator,
                                            ValidatorSignature = block.ValidatorSignature,
                                            Version = block.Version,
                                        };

                                        _context.Block.Add(nBlock);
                                        await _context.SaveChangesAsync();

                                        var txList = new List<Transaction>();
                                        transactions.ForEach(x =>
                                        {
                                            Transaction tx = new Transaction
                                            {
                                                Amount = x.Amount,
                                                BlockId = nBlock.Id,
                                                Fee = x.Fee,
                                                FromAddress = x.FromAddress,
                                                Hash = x.Hash,
                                                Height = x.Height,
                                                NFTData = x.NFTData,
                                                Nonce = x.Nonce,
                                                Signature = x.Signature == null ? "NA" : x.Signature,
                                                Timestamp = x.Timestamp,
                                                ToAddress = x.ToAddress,
                                            };

                                            txList.Add(tx);
                                        });

                                        _context.Transaction.AddRange(txList);
                                        chainData.Height += 1;

                                        await _context.SaveChangesAsync();

                                        string textFin = "finish: ";

                                    }
                                }
                            }
                            else
                            {
                                //failed to call wallet.
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //string textF = "failed: " + ex.Message + " inner: " + ex.InnerException.Message;

                    //await File.WriteAllTextAsync(@"D:\Test\failed.txt", textF);
                }
            }
        }

        private class BlockFromWallet
        {
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

            public IList<TransactionFromWallet> Transactions { get; set; }
        }

        private class TransactionFromWallet
        {
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
        }

    }
}
