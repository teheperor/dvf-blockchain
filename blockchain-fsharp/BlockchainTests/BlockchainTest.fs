open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Expecto
open Blockchain

#nowarn "25"
let tests = 
    test "BlockchainTest" {
        let changeTimestampOfLastBlock (blockchain: Blockchain) timestamp =
            let rec loop chain newRevChain =
                match chain with
                | [] -> newRevChain
                | [lastBlock] -> ({lastBlock with Timestamp = timestamp} :: newRevChain)
                | head :: tail -> loop tail (head :: newRevChain)
            let newRevChain = loop blockchain.Chain []
            blockchain.Chain <- List.rev newRevChain
            newRevChain.Head
            
        let mutable b = Blockchain()
        changeTimestampOfLastBlock b 1562138682.87182<second> |> ignore

        // genesis block
        let block = b.LastBlock ()
        Expect.equal block.Index 1 ""
        Expect.equal block.Proof 100L ""
        Expect.equal block.PreviousHash "1" ""
        Expect.equal block.Transactions.Length 0 ""

        // new transaction
        let result = b.NewTransaction "8527147fe1f5426f9dd545de4b27ee00" "a77f5cdfa2934df3954a5c7c7da5df1f" 5L
        Expect.equal result 2 ""

        // proof of work and new block
        let proof = Blockchain.ProofOfWork (b.LastBlock ())
        let previousHash = Blockchain.Hash (b.LastBlock ())
        Expect.equal proof 32915L ""
        Expect.equal previousHash "f3e700d7bc6124c5f1e91743caddf999f56418363ecdaaead170af052d6abc5a" ""

        b.NewBlock proof previousHash |> ignore
        let block = changeTimestampOfLastBlock b 1562138730.88206<second>
        Expect.equal block.Index 2 ""
        Expect.equal block.Proof proof ""
        Expect.equal block.PreviousHash previousHash ""

        let transactions = block.Transactions
        Expect.equal transactions.Length 1 ""
        let transaction = transactions.[0]
        Expect.equal transaction.Sender "8527147fe1f5426f9dd545de4b27ee00" ""
        Expect.equal transaction.Recipient "a77f5cdfa2934df3954a5c7c7da5df1f" ""
        Expect.equal transaction.Amount 5L ""

        // new transactions
        let result = b.NewTransaction "f5904ab1209feccb054a79783a508e66" "ad9be4f7f91188b5b3509efd7e9973fa" 42L
        Expect.equal result 3 ""
        let result = b.NewTransaction "06246b36066a3f615ec81085c89cbefd" "16b539269e260e2cce7bf9a1d666c78e" 13L
        Expect.equal result 3 ""

        // proof of work and new block
        let proof = Blockchain.ProofOfWork (b.LastBlock ())
        let previousHash = Blockchain.Hash (b.LastBlock ())
        Expect.equal proof 46042L ""
        Expect.equal previousHash "080f2249edf63f86a433a078103c114d5738429b381846e93dc2b846ad62c5a0" ""

        b.NewBlock proof previousHash |> ignore
        let block = changeTimestampOfLastBlock b 1562138771.25776<second>
        Expect.equal block.Index 3 ""
        Expect.equal block.Proof proof ""
        Expect.equal block.PreviousHash previousHash ""

        let transactions = block.Transactions
        Expect.equal transactions.Length 2 ""
        let transaction = transactions.[0]
        Expect.equal transaction.Sender "f5904ab1209feccb054a79783a508e66" ""
        Expect.equal transaction.Recipient "ad9be4f7f91188b5b3509efd7e9973fa" ""
        Expect.equal transaction.Amount 42L ""
        let transaction = transactions.[1]
        Expect.equal transaction.Sender "06246b36066a3f615ec81085c89cbefd" ""
        Expect.equal transaction.Recipient "16b539269e260e2cce7bf9a1d666c78e" ""
        Expect.equal transaction.Amount 13L ""

        // proof of work
        let proof = Blockchain.ProofOfWork (b.LastBlock ())
        let previousHash = Blockchain.Hash (b.LastBlock ())
        Expect.equal proof 202463L ""
        Expect.equal previousHash "49534f78e8cf013ce3ef54e0d3ca3cb5cae021590805052aee75f88e5a457a3b" ""
    }

[<EntryPoint>]
let main args =
    runTestsWithArgs defaultConfig args tests
