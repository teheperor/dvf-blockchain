package io.github.teheperor.blockchain

import kotlin.test.Test
import kotlin.test.assertEquals

class BlockchainTest {
    @Test
    fun testBlockchain() {
        fun changeTimestampOfLastBlock(blockchain: Blockchain, timestamp: Double): Block {
            var chain = blockchain.chain
            val block = chain.removeAt(chain.count() - 1)
            chain.add(Block(block.index, timestamp, block.transactions, block.proof, block.previousHash))
            return chain.last()
        }

        var b = Blockchain()
        changeTimestampOfLastBlock(b, 1562138682.8718169)

        // genesis block
        let {
            val block = b.lastBlock
            assertEquals(1, block.index)
            assertEquals(100, block.proof)
            assertEquals("1", block.previousHash)
            assertEquals(0, block.transactions.count())
        }

        // new transaction
        let {
            val result = b.newTransaction("8527147fe1f5426f9dd545de4b27ee00", "a77f5cdfa2934df3954a5c7c7da5df1f", 5);
            assertEquals(2, result)
        }

        // proof of work and new block
        let {
            val proof = Blockchain.proofOfWork(b.lastBlock)
            val previousHash = Blockchain.hash(b.lastBlock)
//            assertEquals(2964, proof)
//            assertEquals("26aa6d44bd141589a0d3a1a90b86bf411de90d948abab3557b5b30ca75e4e904", previousHash)
            assertEquals(72933, proof)
            assertEquals("90bdb00faa87325f291cbc1b2d6875cfa581cc98b491eee7559527560853b804", previousHash)

            b.newBlock(proof, previousHash)
            val block = changeTimestampOfLastBlock(b, 1562138730.8820634)
            assertEquals(2, block.index)
            assertEquals(proof, block.proof)
            assertEquals(previousHash, block.previousHash)

            val transactions = block.transactions
            assertEquals(1, transactions.count())
            val transaction = transactions[0]
            assertEquals("8527147fe1f5426f9dd545de4b27ee00", transaction.sender)
            assertEquals("a77f5cdfa2934df3954a5c7c7da5df1f", transaction.recipient)
            assertEquals(5, transaction.amount)
        }

        // new transaction
        let {
            var result: Int;
            result = b.newTransaction("f5904ab1209feccb054a79783a508e66", "ad9be4f7f91188b5b3509efd7e9973fa", 42)
            assertEquals(3, result)
            result = b.newTransaction("06246b36066a3f615ec81085c89cbefd", "16b539269e260e2cce7bf9a1d666c78e", 13)
            assertEquals(3, result)
        }

        // proof of work and new block
        let {
            val proof = Blockchain.proofOfWork(b.lastBlock)
            val previousHash = Blockchain.hash(b.lastBlock)
//            assertEquals(2409, proof)
//            assertEquals("7ef1a64eec40e553f39acc252bb3444100d6a95f305e74c2fc40d41d86bbc1d4", previousHash)
            assertEquals(18153, proof)
            assertEquals("eb187c2382022b4beaf0c974536d1e7706125ce5fcdab33c5ab7b4bcb2294e4a", previousHash)

            b.newBlock(proof, previousHash)
            val block = changeTimestampOfLastBlock(b, 1562138771.2577565)
            assertEquals(3, block.index)
            assertEquals(proof, block.proof)
            assertEquals(previousHash, block.previousHash)

            val transactions = block.transactions
            assertEquals(2, transactions.count())
            var transaction: Transaction
            transaction = transactions[0]
            assertEquals("f5904ab1209feccb054a79783a508e66", transaction.sender)
            assertEquals("ad9be4f7f91188b5b3509efd7e9973fa", transaction.recipient)
            assertEquals(42, transaction.amount)
            transaction = transactions[1]
            assertEquals("06246b36066a3f615ec81085c89cbefd", transaction.sender)
            assertEquals("16b539269e260e2cce7bf9a1d666c78e", transaction.recipient)
            assertEquals(13, transaction.amount)
        }

        // proof of work and new block
        let {
            val proof = Blockchain.proofOfWork(b.lastBlock)
            val previousHash = Blockchain.hash(b.lastBlock)
//            assertEquals(28486, proof)
//            assertEquals("2d095dd4fb79711fdb3e780f5ae6ddab1190ddfaa170ee1a1dcb2425bd645652", previousHash)
            assertEquals(118903, proof)
            assertEquals("f9095455b0b191f444d9458fef5c8bb759bc05117e44e6c799de2209eb61a2c2", previousHash)
        }
    }
}
