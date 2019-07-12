using System.Linq;
using FluentAssertions;
using Xunit;

namespace Blockchain.Tests
{
    public class BlockchainTests
    {
        [Fact]
        public void BlockchainTest()
        {
            var b = new Blockchain();
            b.Chain.Last().Timestamp = 1562138682.8718169;

            // genesis block
            {
                var block = b.LastBlock;
                block.Index.Should().Be(1);
                block.Proof.Should().Be(100);
                block.PreviousHash.Should().Be("1");
                block.Transactions.Count.Should().Be(0);
            }

            // new transaction
            {
                var result = b.NewTransaction("8527147fe1f5426f9dd545de4b27ee00", "a77f5cdfa2934df3954a5c7c7da5df1f", 5);
                result.Should().Be(2);
            }

            // proof of work and new block
            {
                var proof = Blockchain.ProofOfWork(b.LastBlock);
                var previousHash = Blockchain.Hash(b.LastBlock);
                proof.Should().Be(2964);
                previousHash.Should().Be("26aa6d44bd141589a0d3a1a90b86bf411de90d948abab3557b5b30ca75e4e904");

                var block = b.NewBlock(proof, previousHash);
                b.Chain.Last().Timestamp = 1562138730.8820634;
                block.Index.Should().Be(2);
                block.Proof.Should().Be(proof);
                block.PreviousHash.Should().Be(previousHash);

                var transactions = block.Transactions;
                transactions.Count.Should().Be(1);
                var transaction = transactions[0];
                transaction.Sender.Should().Be("8527147fe1f5426f9dd545de4b27ee00");
                transaction.Recipient.Should().Be("a77f5cdfa2934df3954a5c7c7da5df1f");
                transaction.Amount.Should().Be(5);
            }

            // new transactions
            {
                int result;
                result = b.NewTransaction("f5904ab1209feccb054a79783a508e66", "ad9be4f7f91188b5b3509efd7e9973fa", 42);
                result.Should().Be(3);
                result = b.NewTransaction("06246b36066a3f615ec81085c89cbefd", "16b539269e260e2cce7bf9a1d666c78e", 13);
                result.Should().Be(3);
            }

            // proof of work and new block
            {
                var proof = Blockchain.ProofOfWork(b.LastBlock);
                var previousHash = Blockchain.Hash(b.LastBlock);
                proof.Should().Be(2409);
                previousHash.Should().Be("7ef1a64eec40e553f39acc252bb3444100d6a95f305e74c2fc40d41d86bbc1d4");

                var block = b.NewBlock(proof, previousHash);
                b.Chain.Last().Timestamp = 1562138771.2577565;
                block.Index.Should().Be(3);
                block.Proof.Should().Be(proof);
                block.PreviousHash.Should().Be(previousHash);

                var transactions = block.Transactions;
                transactions.Count.Should().Be(2);
                Transaction transaction;
                transaction = transactions[0];
                transaction.Sender.Should().Be("f5904ab1209feccb054a79783a508e66");
                transaction.Recipient.Should().Be("ad9be4f7f91188b5b3509efd7e9973fa");
                transaction.Amount.Should().Be(42);
                transaction = transactions[1];
                transaction.Sender.Should().Be("06246b36066a3f615ec81085c89cbefd");
                transaction.Recipient.Should().Be("16b539269e260e2cce7bf9a1d666c78e");
                transaction.Amount.Should().Be(13);
            }

            // proof of work
            {
                var proof = Blockchain.ProofOfWork(b.LastBlock);
                var previousHash = Blockchain.Hash(b.LastBlock);
                proof.Should().Be(28486);
                previousHash.Should().Be("2d095dd4fb79711fdb3e780f5ae6ddab1190ddfaa170ee1a1dcb2425bd645652");
            }
        }
    }
}