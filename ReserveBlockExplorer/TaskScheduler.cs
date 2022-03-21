using Microsoft.AspNetCore.SignalR.Client;
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
        private Timer masterNodeTimer;
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

            masterNodeTimer = new Timer(o =>
            {
                MasternodeChecks();
            },
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromHours(3));


            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Error");

            return Task.CompletedTask;
        }

        public async void MasternodeChecks()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var validators = _context.Validator.Where(x => x.FailCount < 10).ToList();

                var hr24OldBlocks = _context.Block.Where(x => x.Timestamp >= GetTime(-24));
                var weekOldBlocks = _context.Block.Where(x => x.Timestamp >= GetTime(-168));

                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (HttpClient client = new HttpClient(clientHandler))
                {
                    string endpoint = walletAPIURL + "/api/V1/GetMasternodes";
                    using (var Response = await client.GetAsync(endpoint))
                    {
                        if (Response.StatusCode == HttpStatusCode.OK)
                        {
                            string data = await Response.Content.ReadAsStringAsync();
                            var validatorList = JsonConvert.DeserializeObject<List<Models.Validator>>(data);

                            var newValidators = validatorList.Where(x => !validators.Any(y => x.Address == y.Address)).ToList();

                            var valInsterList = new List<Models.Validator>();

                            foreach (var validator in newValidators)
                            {
                                Models.Validator val = new Models.Validator();

                                val.UniqueName = validator.UniqueName;
                                val.Address = validator.Address;
                                val.FailCount = validator.FailCount;
                                val.NodeReferenceId = validator.NodeReferenceId;
                                val.IsActive = validator.IsActive;
                                val.Amount = validator.Amount;
                                val.EligibleBlockStart = validator.EligibleBlockStart;
                                val.NodeIP = validator.NodeIP;
                                val.Position = validator.Position;
                                val.Signature = validator.Signature;

                                valInsterList.Add(val); 
                            }

                            if(valInsterList.Count > 0)
                            {
                                _context.Validator.AddRange(valInsterList);
                                _context.SaveChanges();
                            }
                        }
                    }
                }

                var valList = _context.Validator.Where(x => x.FailCount < 10).ToList();

                foreach(var val in valList)
                {
                    var hubConnection1 = new HubConnectionBuilder()
                        .WithUrl("http://" + val.NodeIP + ":3338/blockchain")
                        .Build();
                    //online check
                    var valInfo = _context.ValidatorInfo.Where(x => x.Address == val.Address).FirstOrDefault();
                    try
                    {
                        var alive = hubConnection1.StartAsync().Wait(8000);
                        if (alive == true)
                        {
                            var online = await hubConnection1.InvokeAsync<bool>("MasternodeOnline");
                            if(online == true)
                            {
                                
                                if(valInfo == null)
                                {
                                    ValidatorInfo nValInfo = new ValidatorInfo {
                                        Address = val.Address,
                                        FailChecks = 0,
                                        IsBanned = false,
                                        IsOnline = true,
                                        LastChecked = DateTime.UtcNow,
                                        ValidatorId = val.Id,
                                        BlocksIn24Hours = hr24OldBlocks.Where(x => x.Validator == val.Address).ToList().Count(),
                                        BlocksIn7Days = weekOldBlocks.Where(x => x.Validator == val.Address).ToList().Count()
                                    };

                                    _context.ValidatorInfo.Add(nValInfo);
                                }
                                else
                                {
                                    valInfo.LastChecked = DateTime.UtcNow;
                                    valInfo.IsOnline = true;
                                    valInfo.BlocksIn24Hours = hr24OldBlocks.Where(x => x.Validator == val.Address).ToList().Count();
                                    valInfo.BlocksIn7Days = weekOldBlocks.Where(x => x.Validator == val.Address).ToList().Count();
                                }

                                _context.SaveChanges();

                                validators = await hubConnection1.InvokeAsync<List<Models.Validator>?>("GetBannedMasternodes");

                                if (validators != null)
                                {
                                    foreach (var validator in validators)
                                    {
                                        var validatorInfo = _context.ValidatorInfo.Where(x => x.Address == validator.Address).FirstOrDefault();
                                        if(validatorInfo == null)
                                        {
                                            ValidatorInfo nValInfo = new ValidatorInfo
                                            {
                                                Address = val.Address,
                                                FailChecks = 0,
                                                IsBanned = true,
                                                IsOnline = true,
                                                LastChecked = DateTime.UtcNow,
                                                ValidatorId = val.Id,
                                                BlocksIn24Hours = hr24OldBlocks.Where(x => x.Validator == val.Address).ToList().Count(),
                                                BlocksIn7Days = weekOldBlocks.Where(x => x.Validator == val.Address).ToList().Count()
                                            };
                                        }
                                        else
                                        {
                                            validatorInfo.IsBanned = true;
                                        }

                                        var banReport = _context.BanReport.Where(x => x.BannedAddress == validator.Address).FirstOrDefault();
                                        if(banReport == null)
                                        {
                                            BanReport nBanReport = new BanReport { 
                                                BannedAddress = validator.Address,
                                                ReportDate = DateTime.UtcNow,
                                                ReporterAddress = val.Address,
                                            };
                                        }

                                        _context.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                if (valInfo == null)
                                {
                                    ValidatorInfo nValInfo = new ValidatorInfo
                                    {
                                        Address = val.Address,
                                        FailChecks = 0,
                                        IsBanned = false,
                                        IsOnline = false,
                                        LastChecked = DateTime.UtcNow,
                                        ValidatorId = val.Id,
                                        BlocksIn24Hours = hr24OldBlocks.Where(x => x.Validator == val.Address).ToList().Count(),
                                        BlocksIn7Days = weekOldBlocks.Where(x => x.Validator == val.Address).ToList().Count()
                                    };

                                    _context.ValidatorInfo.Add(nValInfo);
                                }
                                else
                                {
                                    valInfo.LastChecked = DateTime.UtcNow;
                                    valInfo.IsOnline = false;
                                }
                                _context.SaveChanges();
                            }   
                        }
                        else
                        {
                            if (valInfo == null)
                            {
                                ValidatorInfo nValInfo = new ValidatorInfo
                                {
                                    Address = val.Address,
                                    FailChecks = 0,
                                    IsBanned = false,
                                    IsOnline = false,
                                    LastChecked = DateTime.UtcNow,
                                    ValidatorId = val.Id
                                };

                                _context.ValidatorInfo.Add(nValInfo);
                            }
                            else
                            {
                                valInfo.LastChecked = DateTime.UtcNow;
                                valInfo.IsOnline = false;
                                valInfo.BlocksIn24Hours = hr24OldBlocks.Where(x => x.Validator == val.Address).ToList().Count();
                                valInfo.BlocksIn7Days = weekOldBlocks.Where(x => x.Validator == val.Address).ToList().Count();
                            }
                            _context.SaveChanges();
                        }
                    }
                    catch(Exception ex)
                    {
                        if (valInfo == null)
                        {
                            ValidatorInfo nValInfo = new ValidatorInfo
                            {
                                Address = val.Address,
                                FailChecks = 0,
                                IsBanned = false,
                                IsOnline = false,
                                LastChecked = DateTime.UtcNow,
                                ValidatorId = val.Id
                            };

                            _context.ValidatorInfo.Add(nValInfo);
                        }
                        else
                        {
                            valInfo.LastChecked = DateTime.UtcNow;
                            valInfo.IsOnline = false;
                            valInfo.BlocksIn24Hours = hr24OldBlocks.Where(x => x.Validator == val.Address).ToList().Count();
                            valInfo.BlocksIn7Days = weekOldBlocks.Where(x => x.Validator == val.Address).ToList().Count();
                        }
                        _context.SaveChanges();
                    }
                }    

            }
        }

        public static long GetTime(int timeAdd = 0)
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long nowTicks = DateTime.UtcNow.AddHours(timeAdd).Ticks;
            long timeStamp = ((nowTicks - epochTicks) / TimeSpan.TicksPerSecond);
            return timeStamp;

            //returns time in ticks from Epoch Time
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
