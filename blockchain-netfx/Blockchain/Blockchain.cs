using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Blockchain
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Transaction
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public long Amount { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Block
    {
        public int Index { get; set; }
        public double Timestamp { get; set; }
        public IList<Transaction> Transactions { get; set; }
        public long Proof { get; set; }
        public string PreviousHash { get; set; }
    }

    public class Blockchain
    {
        private IList<Transaction> CurrentTransactions { get; set; } = new List<Transaction>();
        public IList<Block> Chain { get; set; } = new List<Block>();
        public HashSet<string> Nodes { get; set; } = new HashSet<string>();

        public Blockchain() => NewBlock(previousHash: "1", proof: 100);

        public void RegisterNode(string address)
        {
            if (!address.StartsWith("http://"))
                address = $"http://{address}";
            if (Uri.TryCreate(address, UriKind.Absolute, out var parsedUrl))
                Nodes.Add(parsedUrl.Authority);
            else
                throw new UriFormatException("Invalid URL");
        }

        static public bool ValidChain(IList<Block> chain)
        {
            var lastBlock = chain[0];
            var currentIndex = 1;

            while (currentIndex < chain.Count)
            {
                var block = chain[currentIndex];
                Console.WriteLine(JsonConvert.SerializeObject(lastBlock));
                Console.WriteLine(JsonConvert.SerializeObject(block));
                Console.WriteLine();
                Console.WriteLine("--------------");

                var lastBlockHash = Hash(lastBlock);
                if (block.PreviousHash != lastBlockHash)
                    return false;

                if (!ValidProof(lastBlock.Proof, block.Proof, lastBlockHash))
                    return false;

                lastBlock = block;
                currentIndex++;
            }

            return true;
        }

        public async Task<bool> ResolveConflicts()
        {
            var neighbours = Nodes;
            IList<Block> newChain = null;

            var maxLength = Chain.Count;

            foreach (var node in neighbours)
            {
                var client = new HttpClient();
                var response = await client.GetAsync($"http://{node}/chain");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var messageBody = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(messageBody);
                    var length = (int)json["length"];
                    var chain = json["chain"].ToObject<IList<Block>>();

                    if (length > maxLength && ValidChain(chain))
                    {
                        maxLength = length;
                        newChain = chain;
                    }
                }
            }

            if (newChain != null)
            {
                Chain = newChain;
                return true;
            }

            return false;
        }

        public Block NewBlock(long proof, string previousHash)
        {
            var block = new Block
            {
                Index = Chain.Count + 1,
                Timestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1.0e-3,
                Transactions = CurrentTransactions,
                Proof = proof,
                PreviousHash = string.IsNullOrEmpty(previousHash) ? Hash(Chain.Last()) : previousHash,
            };

            CurrentTransactions = new List<Transaction>();

            Chain.Add(block);
            return block;
        }

        public int NewTransaction(string sender, string recipient, long amount)
        {
            CurrentTransactions.Add(new Transaction { Sender = sender, Recipient = recipient, Amount = amount });
            return LastBlock.Index + 1;
        }

        public Block LastBlock => Chain.Last();

        public static string Hash(Block block)
        {
            var blockString = JsonConvert.SerializeObject(block);
            using (var sha256 = SHA256Managed.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static long ProofOfWork(Block lastBlock)
        {
            var lastProof = lastBlock.Proof;
            var lastHash = Hash(lastBlock);

            var proof = 0L;
            while (!ValidProof(lastProof, proof, lastHash))
                proof++;

            return proof;
        }

        public static bool ValidProof(long lastProof, long proof, string lastHash)
        {
            var guess = $"{lastProof.ToString()}{proof.ToString()}{lastHash}";
            using (var sha256 = SHA256Managed.Create())
            {
                var guessHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(guess));
                var guessHash = BitConverter.ToString(guessHashBytes).Replace("-", "").ToLower();
                return guessHash.StartsWith("0000");
            }
        }
    }
}
