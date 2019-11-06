import com.beust.klaxon.Json
import com.beust.klaxon.JsonObject
import com.beust.klaxon.Klaxon
import com.beust.klaxon.Parser
import com.fasterxml.jackson.annotation.JsonProperty
import com.github.kittinunf.fuel.httpGet
import java.lang.StringBuilder
import java.net.MalformedURLException
import java.net.URL
import java.security.MessageDigest
import java.time.Instant

data class Transaction(val sender: String, val recipient: String, val amount: Long)

data class Block(
    val index: Int,
    val timestamp: Double,
    val transactions: MutableList<Transaction>,
    val proof: Long,
    @Json(name = "previous_hash") @JsonProperty("previous_hash") val previousHash: String
)

class Blockchain() {
    private var currentTransactions: MutableList<Transaction> = mutableListOf()
    var chain: MutableList<Block> = mutableListOf()
    var nodes: MutableSet<String> = mutableSetOf()

    init {
        newBlock(previousHash = "1", proof = 100)
    }

    /**
     * Add a new node to the list of nodes
     *
     * @param address Address of node. Eg. 'http://192.168.0.5:5000'
     */
    fun registerNode(address: String) {
        val url = URL(if (address.startsWith("http://")) address else "http://${address}")
        if (!url.host.isEmpty())
            nodes.add(url.authority)
        else
            throw MalformedURLException("Invalid URL")
    }

    /**
     * This is our consensus algorithm, it resolves conflicts by replacing our chain with the longest one in the network.
     *
     * @return true if our chain was replaced, false if not
     */
    fun resolveConflicts(): Boolean {
        val neighbors = nodes
        var newChain: List<Block>? = null

        var maxLength = chain.count()

        for (node in neighbors) {
            val (_, response, _) = "http://$node/chain".httpGet().responseString()

            if (response.statusCode == 200) {
                val json = Parser.default().parse(StringBuilder(String(response.data))) as JsonObject
                val length = json.int("length")!!
                val chain = Klaxon().parseFromJsonArray<Block>(json.array<JsonObject>("chain")!!)!!

                if (length > maxLength && validChain(chain)) {
                    maxLength = length
                    newChain = chain
                }
            }
        }

        if (newChain != null) {
            chain = newChain.toMutableList()
            return true
        }

        return false
    }

    /**
     * Create a new Block in the Blockchain
     *
     * @param proof The proof given by the Proof of Work algorithm
     * @param previousHash Hash of previous Block
     * @return New Block
     */
    fun newBlock(proof: Long, previousHash: String): Block {
        val instant = Instant.now()
        val block = Block(
            index = chain.count() + 1,
            timestamp = instant.epochSecond.toDouble() + instant.nano * 1.0e-9,
            transactions = this.currentTransactions,
            proof = proof,
            previousHash = if (previousHash.isEmpty()) hash(chain.last()) else previousHash
        )

        currentTransactions = mutableListOf()

        chain.add(block)
        return block
    }

    /**
     * Creates a new transaction to go into the next mined Block
     *
     * @param sender Address of the Sender
     * @param recipient Address of the Recipient
     * @param amount Amount
     * @return The index of the Block that will hold this transaction
     */
    fun newTransaction(sender: String, recipient: String, amount: Long): Int {
        currentTransactions.add(Transaction(sender, recipient, amount))

        return lastBlock.index + 1
    }

    val lastBlock: Block
        get() = chain.last()

    companion object {
        /**
         * Determine if a given blockchain is valid
         *
         * @param chain A blockchain
         * @return true if valid, false if not
         */
        @JvmStatic
        fun validChain(chain: List<Block>): Boolean {
            var lastBlock = chain.first()
            var currentIndex = 1

            while (currentIndex < chain.count()) {
                val block = chain[currentIndex]
                println(Klaxon().toJsonString(lastBlock))
                println(Klaxon().toJsonString(block))
                println("\n--------------\n")

                val lastBlockHash = hash(lastBlock)
                if (block.previousHash != lastBlockHash)
                    return false

                if (!validProof(lastBlock.proof, block.proof, lastBlockHash))
                    return false

                lastBlock = block
                currentIndex++
            }

            return true
        }

        /**
         * Creates a SHA-256 hash of a Block
         *
         * @param block Block
         */
        @JvmStatic
        fun hash(block: Block): String {
            val blockString = Klaxon().toJsonString(block)
            return MessageDigest
                .getInstance("SHA-256")
                .digest(blockString.toByteArray())
                .joinToString(separator = "") { "%02x".format(it) }
        }

        /**
         * Simple Proof of Work Algorithm:
         *
         * - Find a number p' such that hash(pp') contains leading 4 zeroes
         * - Where p is the previous proof, and p' is the new proof
         *
         * @param lastBlock last Block
         * @return
         */
        @JvmStatic
        fun proofOfWork(lastBlock: Block): Long {
            val lastProof = lastBlock.proof
            val lastHash = hash(lastBlock)

            var proof = 0L
            while (!validProof(lastProof, proof, lastHash))
                proof++

            return proof
        }

        /**
         * Validates the Proof
         *
         * @param lastProof Previous Proof
         * @param proof Current Proof
         * @param lastHash The hash of the Previous Block
         * @return true if correct, false if not.
         */
        @JvmStatic
        fun validProof(lastProof: Long, proof: Long, lastHash: String): Boolean {
            val guess = "$lastProof$proof$lastHash"
            val guessHash = MessageDigest
                .getInstance("SHA-256")
                .digest(guess.toByteArray())
                .joinToString(separator = "") { "%02x".format(it) }
            return guessHash.startsWith("0000")
//            return guessHash.startsWith("000")
        }
    }
}
