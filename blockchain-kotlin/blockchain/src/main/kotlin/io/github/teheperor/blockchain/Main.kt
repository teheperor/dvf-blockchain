package io.github.teheperor.blockchain

import com.xenomachina.argparser.ArgParser
import com.xenomachina.argparser.default
import com.xenomachina.argparser.mainBody
import io.ktor.application.*
import io.ktor.features.*
import io.ktor.http.*
import io.ktor.jackson.*
import io.ktor.request.*
import io.ktor.response.*
import io.ktor.routing.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*
import org.slf4j.event.Level
import java.util.UUID

class Args(parser: ArgParser) {
    val port by parser.storing("-p", "--port", help = "port to listen on") { toInt() }.default(5000)
}

fun main(args: Array<String>): Unit = mainBody {
    val parseArgs = ArgParser(args).parseInto(::Args)
    val port = parseArgs.port

    val nodeIdentifier = UUID.randomUUID().toString().replace("-", "")

    var blockchain = Blockchain()

    val server = embeddedServer(Netty, port) {
        install(CallLogging) {
            level = Level.INFO
        }
        install(ContentNegotiation) {
            jackson { }
        }
        routing {
            get("/mine") {
                val lastBlock = blockchain.lastBlock
                val proof = Blockchain.proofOfWork(lastBlock)

                blockchain.newTransaction(
                    sender = "0",
                    recipient = nodeIdentifier,
                    amount = 1
                )

                val previousHash = Blockchain.hash(lastBlock)
                val block = blockchain.newBlock(proof, previousHash)

                val response = object {
                    val message = "New Block Forged"
                    val index = block.index
                    val transactions = block.transactions
                    val proof = block.proof
                    val previous_hash = block.previousHash
                }
                call.respond(HttpStatusCode.OK, response)
            }

            post("/transactions/new") {
                val values = call.receive<Map<String, Any>>()

                val required = arrayOf("sender", "recipient", "amount")
                if (!required.all { values.containsKey(it) })
                    call.respond(HttpStatusCode.BadRequest, "Missing Values")

                val index = blockchain.newTransaction(
                    values["sender"] as String, values["recipient"] as String, (values["amount"] as Int).toLong()
                )

                val response = object {
                    val message = "Transaction will be added to Block $index"
                }
                call.respond(HttpStatusCode.Created, response)
            }

            get("/chain") {
                val response = object {
                    val chain = blockchain.chain
                    val length = blockchain.chain.count()
                }
                call.respond(HttpStatusCode.OK, response)
            }

            post("/nodes/register") {
                val values = call.receive<Map<String, Any>>()

                val nodes = values["nodes"]
                if (nodes == null)
                    call.respond(HttpStatusCode.BadRequest, "Error: Please supply a valid list of nodes")

                @Suppress("UNCHECKED_CAST")
                for (node in nodes as Iterable<String>)
                    blockchain.registerNode(node)

                val response = object {
                    val message = "New nodes have been added"
                    val total_nodes = blockchain.nodes
                }
                call.respond(HttpStatusCode.Created, response)
            }

            get("/nodes/resolve") {
                val replaced = blockchain.resolveConflicts()

                @Suppress("IMPLICIT_CAST_TO_ANY")
                val response =
                    if (replaced)
                        object {
                            val message = "Our chain was replaced"
                            val new_chain = blockchain.chain
                        }
                    else
                        object {
                            val message = "Our chain is authoritative"
                            val chain = blockchain.chain
                        }

                call.respond(HttpStatusCode.OK, response)
            }
        }
    }
    server.start(wait = true)
}
