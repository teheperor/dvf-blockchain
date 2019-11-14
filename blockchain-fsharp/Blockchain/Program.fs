open System
open System.Text
open Argu
open Chiron
open Suave
open Suave.Filters
open Suave.Logging
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful
open Blockchain

type Arguments =
    | [<AltCommandLine("-p")>] Port of port: int
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Port _ -> "port to listen on"

let nodeIdentifier = Guid.NewGuid().ToString().Replace("-", "")

let mutable blockchain = Blockchain()

let logger = Targets.create Info [||]
let app =
    choose [
        GET >=> path "/mine" >=> request (fun _ ->
            let lastBlock = blockchain.LastBlock ()
            let proof = Blockchain.ProofOfWork lastBlock

            blockchain.NewTransaction "0" nodeIdentifier 1L |> ignore

            let previousHsah = Blockchain.Hash lastBlock
            let block = blockchain.NewBlock proof previousHsah

            let response = Object <| Map.ofList [
                "message", String "New Block Forged"
                "index", Number (decimal block.Index)
                "transactions", Json.serialize block.Transactions
                "proof", Number (decimal block.Proof)
                "previous_hash", String block.PreviousHash
            ]
            Writers.setMimeType "application/json; charset=UTF-8" >=>
            OK (Json.format response))

        POST >=> path "/transactions/new" >=> request (fun req ->
            let rec loop (values: Map<string, Json>) required =
                match required with
                | [] -> 
                    let index = blockchain.NewTransaction 
                                    (Json.deserialize values.["sender"]) 
                                    (Json.deserialize values.["recipient"]) 
                                    (Json.deserialize values.["amount"])

                    let response = Object(Map[("message", String(String.Format("Transaction will be added to Block {0}", index)))])
                    Writers.setMimeType "application/json; charset=UTF-8" >=>
                    CREATED (Json.format response)
                | head :: tail ->
                    if values.ContainsKey(head)
                    then loop values tail
                    else BAD_REQUEST "Missing values"

            let values: Map<string, Json> = req.rawForm |> Encoding.UTF8.GetString |> Json.parse |> Json.deserialize
            let required = ["sender"; "recipient"; "amount"]
            loop values required)

        GET >=> path "/chain" >=> request (fun _ ->
            let response = Object <| Map.ofList [
                "chain", Json.serialize blockchain.Chain
                "length", Number (decimal blockchain.Chain.Length)
            ]
            Writers.setMimeType "application/json; charset=UTF-8" >=>
            OK (Json.format response))

        POST >=> path "/nodes/register" >=> request (fun req ->
            let values: Map<string, Json> = req.rawForm |> Encoding.UTF8.GetString |> Json.parse |> Json.deserialize

            match values.TryGetValue "nodes" with
            | true, Array(nodes) ->
                let rec loop nodes =
                    match nodes with
                    | [] ->
                        let response = Object <| Map.ofList [
                            "message", String "New nodes have been added"
                            "total_nodes", Json.serialize blockchain.Nodes
                        ]
                        Writers.setMimeType "application/json; charset=UTF-8" >=>
                        CREATED (Json.format response)
                    | head :: tail ->
                        head |> Json.deserialize |> blockchain.RegisterNode
                        loop tail
                loop nodes
            | _ -> 
                BAD_REQUEST "Error: Please supply a valid list of nodes")

        GET >=> path "/nodes/resolve" >=> request (fun _ ->
            let replaced = blockchain.ResolveConflicts ()

            let response = 
                if replaced
                then (Object <| Map.ofList [
                    "message", String "Our chain was replaced"
                    "new_chain", Json.serialize blockchain.Chain
                ])
                else (Object <| Map.ofList [
                    "message", String "Our chain is authoritative"
                    "chain", Json.serialize blockchain.Chain
                ])

            Writers.setMimeType "application/json; charset=UTF-8" >=>
            OK (Json.format response))

        RequestErrors.NOT_FOUND "Found no handlers"
    ] >=> logWithLevelStructured Info logger logFormatStructured

[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    let results = parser.Parse argv
    let port = results.GetResult (Port, defaultValue = 5000)

    let config = {
        defaultConfig with
            bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" port ]
    }
    startWebServer config app
    0
