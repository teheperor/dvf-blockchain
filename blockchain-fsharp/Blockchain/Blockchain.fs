namespace Blockchain

open System
open System.Net
open System.Net.Http
open System.Security.Cryptography
open System.Text
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Chiron

type Transaction = {
    Sender: string
    Recipient: string
    Amount: int64
} with

static member ToJson (t: Transaction) = json {
    do! Json.write "sender" t.Sender
    do! Json.write "recipient" t.Recipient
    do! Json.write "amount" t.Amount
}

static member FromJson (_: Transaction) = json {
    let! sender = Json.read "sender"
    let! recipient = Json.read "recipient"
    let! amount = Json.read "amount"

    return {
        Transaction.Sender = sender
        Transaction.Recipient = recipient
        Transaction.Amount = amount
    }
}
end

type Block = {
    Index: int
    Timestamp: float<second>
    Transactions: List<Transaction>
    Proof: int64
    PreviousHash: string
} with

static member ToJson (b: Block) = json {
    do! Json.write "index" b.Index
    do! Json.write "timestamp" (float b.Timestamp)
    do! Json.write "transactions" b.Transactions
    do! Json.write "proof" b.Proof
    do! Json.write "previous_hash" b.PreviousHash
}

static member FromJson (_: Block) = json {
    let! index = Json.read "index"
    let! timestamp= Json.read "timestamp"
    let! transactions = Json.read "transactions"
    let! proof = Json.read "proof"
    let! previousHash = Json.read "previous_hash"

    return {
        Block.Index = index
        Block.Timestamp = LanguagePrimitives.FloatWithMeasure<second> timestamp
        Block.Transactions = transactions
        Block.Proof = proof
        Block.PreviousHash = previousHash
    }
}
end

type Blockchain private (_unused_) =
    let mutable currentTransactions = []
    static let httpClient = new HttpClient()

    new () as this = 
        Blockchain(())
        then
            this.NewBlock 100L "1" |> ignore

    member val Chain = [] with get, set
    member val Nodes = set [] with get, set

    /// <summary>
    /// Create a new Block in the Blockchain
    /// </summary>
    /// <param name="proof">The proof given by the Proof of Work algorithm</param>
    /// <param name="previousHash">Hash of previous Block</param>
    /// <returns>New Block</returns>
    member this.NewBlock proof previousHash =
        let block = {
            Index = this.Chain.Length + 1
            Timestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1.0e-3<second>
            Transactions = currentTransactions
            Proof = proof
            PreviousHash = if String.IsNullOrEmpty previousHash then this.Chain |> List.last |> Blockchain.Hash else previousHash
        }

        currentTransactions <- []

        this.Chain <- this.Chain @ [ block ]
        block

    member this.LastBlock() = List.last this.Chain

    /// <summary>
    /// Creates a new transaction to go into the next mined Block
    /// </summary>
    /// <param name="sender">Address of the Sender</param>
    /// <param name="recipient">Address of the Recipient</param>
    /// <param name="amount">Amount</param>
    /// <returns>The index of the Block that will hold this transaction</returns>
    member this.NewTransaction sender recipient amount =
        currentTransactions <- currentTransactions @ [ { Sender = sender; Recipient = recipient; Amount = amount } ]

        this.LastBlock().Index + 1

    /// <summary>
    /// Creates a SHA-256 hash of a Block
    /// </summary>
    /// <param name="block">Block</param>
    /// <returns></returns>
    static member Hash block =
        let blockString = block |> Json.serialize |> Json.format
        use sha256 = SHA256Managed.Create()
        let hashBytesString = blockString |> Encoding.UTF8.GetBytes |> sha256.ComputeHash |> BitConverter.ToString
        hashBytesString.Replace("-", "").ToLower()

    /// <summary>
    /// Validates the Proof
    /// </summary>
    /// <param name="lastProof">Previous Proof</param>
    /// <param name="proof">Current Proof</param>
    /// <param name="lastHash">The hash of the Previous Block</param>
    /// <returns>true if correct, false if not.</returns>
    static member ValidProof lastProof proof lastHash =
        let guess = String.Format("{0}{1}{2}", lastProof, proof, lastHash)
        use sha256 = SHA256Managed.Create()
        let guessHashBytesString = guess |> Encoding.UTF8.GetBytes |> sha256.ComputeHash |> BitConverter.ToString
        let guessHash = guessHashBytesString.Replace("-", "").ToLower();
        //guessHash.StartsWith "000000";
        guessHash.StartsWith "0000";

    /// <summary>
    /// Simple Proof of Work Algorithm:
    /// 
    /// - Find a number p' such that hash(pp') contains leading 4 zeroes
    /// - Where p is the previous proof, and p' is the new proof
    /// </summary>
    /// <param name="lastBlock">last Block</param>
    /// <returns></returns>
    static member ProofOfWork lastBlock =
        let rec loop lastProof proof lastHash =
            if Blockchain.ValidProof lastProof proof lastHash
            then proof
            else loop lastProof (proof + 1L) lastHash

        let lastProof = lastBlock.Proof
        let lastHash = Blockchain.Hash lastBlock
        loop lastProof 0L lastHash

    /// <summary>
    /// Add a new node to the list of nodes
    /// </summary>
    /// <param name="address">Address of node. Eg. 'http://192.168.0.5:5000'</param>
    member this.RegisterNode (address: string) =
        let address = if address.StartsWith("http://") then address else "http://" + address
        match Uri.TryCreate(address, UriKind.Absolute) with
        | true, uri -> this.Nodes <- this.Nodes.Add uri.Authority
        | _ -> raise <| UriFormatException("Invalid URL")

    /// <summary>
    /// Determine if a given blockchain is valid
    /// </summary>
    /// <param name="chain">A blockchain</param>
    /// <returns>true if valid, false if not</returns>
    static member ValidChain (chain: List<Block>) =
        let rec loop lastBlock chain =
            match chain with
            | [] -> true
            | head :: tail ->
                let block = head
                printfn "%s" (lastBlock |> Json.serialize |> Json.format)
                printfn "%s" (block |> Json.serialize |> Json.format)
                printfn "\n-----------\n"

                let lastBlockHash = Blockchain.Hash lastBlock
                if block.PreviousHash <> lastBlockHash || not (Blockchain.ValidProof lastBlock.Proof block.Proof lastBlockHash)
                then false
                else loop block tail

        let lastBlock = chain.Head
        let chain = chain.Tail
        loop lastBlock chain

    /// <summary>
    /// This is our consensus algorithm, it resolves conflicts by replacing our chain with the longest one in the network.
    /// </summary>
    /// <returns>true if our chain was replaced, false if not</returns>
    member this.ResolveConflicts () =
        let getAsync (url: string) =
            async {
                let! response = httpClient.GetAsync(url) |> Async.AwaitTask
                response.EnsureSuccessStatusCode () |> ignore
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content
            }
        let rec loop neighbors newChain maxLength =
            match neighbors with
            | [] ->
                match newChain with
                | Some chain ->
                    this.Chain <- chain
                    true
                | _ ->
                    false
            | head :: tail ->
                let messageBody = getAsync ("http://" + head + "/chain") |> Async.RunSynchronously
                let jsonMap: Map<string, Json> = messageBody |> Json.parse |> Json.deserialize
                let length: int = Json.deserialize jsonMap.["length"]
                let chain: List<Block> = Json.deserialize jsonMap.["chain"]

                if length > maxLength && Blockchain.ValidChain chain
                then loop tail (Some chain) length
                else loop tail newChain maxLength

        let neighbors = Set.toList this.Nodes
        let newChain = None
        let maxLength = this.Chain.Length
        loop neighbors newChain maxLength
