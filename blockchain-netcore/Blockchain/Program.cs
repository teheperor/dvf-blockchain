using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy;
using Nancy.Extensions;
using Nancy.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blockchain
{
    public class Program : NancyModule
    {
        private static readonly string nodeIdentifier = Guid.NewGuid().ToString().Replace("-", "");

        private static readonly Blockchain blockchain = new Blockchain();

        public Program()
        {
            Get("/mine", _ =>
            {
                var lastBlock = blockchain.LastBlock;
                var proof = Blockchain.ProofOfWork(lastBlock);

                blockchain.NewTransaction("0", nodeIdentifier, 1);

                var previousHash = Blockchain.Hash(lastBlock);
                var block = blockchain.NewBlock(proof, previousHash);

                var response = new JObject
                {
                    { "message", "New Block Forged" },
                    { "index", block.Index },
                    { "transactions", JArray.FromObject(block.Transactions) },
                    { "proof",  block.Proof },
                    { "previous_hash", block.PreviousHash },
                };
                return ((Response)(response.ToString(Formatting.None)))
                    .WithStatusCode(HttpStatusCode.OK).WithContentType("application/json");
            });

            Post("/transactions/new", _ =>
            {
                var values = JObject.Parse(Request.Body.AsString());
                var required = new[] { "sender", "recipient", "amount" };
                if (required.Any(x => values[x] == null))
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithModel("Missing values");

                var index = blockchain.NewTransaction((string)values["sender"], (string)values["recipient"], (long)values["amount"]);

                var response = new JObject { { "message", $"Transaction will be added to Block {index}" } };
                return ((Response)(response.ToString(Formatting.None)))
                    .WithStatusCode(HttpStatusCode.Created).WithContentType("application/json");
            });

            Get("/chain", _ =>
            {
                var response = new JObject
                {
                    { "chain", JArray.FromObject(blockchain.Chain) },
                    { "length", blockchain.Chain.Count },
                };
                return ((Response)(response.ToString(Formatting.None)))
                    .WithStatusCode(HttpStatusCode.OK).WithContentType("application/json");
            });

            Post("/nodes/register", _ =>
            {
                var values = JObject.Parse(Request.Body.AsString());

                var nodes = values["nodes"].ToObject<IList<string>>();
                if (nodes == null)
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithModel("Error: Please supply a valid list of nodes");

                foreach (var node in nodes)
                    blockchain.RegisterNode(node);

                var response = new JObject
                {
                    { "message", "New nodes have been added" },
                    { "total_nodes", JArray.FromObject(blockchain.Nodes) },
                };
                return ((Response)(response.ToString(Formatting.None)))
                    .WithStatusCode(HttpStatusCode.Created).WithContentType("application/json");
            });

            Get("/nodes/resolve", async _ =>
            {
                var replaced = await blockchain.ResolveConflicts();

                var response =
                    replaced ?
                    new JObject
                    {
                        { "message", "Our chain was replaced" },
                        { "new_chain", JArray.FromObject(blockchain.Chain) },
                    } :
                    new JObject
                    {
                        { "message", "Our chain is authoritative" },
                        { "chain", JArray.FromObject(blockchain.Chain) },
                    };

                return ((Response)(response.ToString(Formatting.None)))
                    .WithStatusCode(HttpStatusCode.OK).WithContentType("application/json");
            });
        }

        static void Main(string[] args)
        {
            var port = 0;
            {
                var app = new CommandLineApplication();
                app.HelpOption("-?|--help");
                var optionPort = app.Option<int>("-p|--port <PORT>", "port to listen on", CommandOptionType.SingleValue);
                app.OnExecute(() => {
                    port = optionPort.HasValue() ? optionPort.ParsedValue : 5000;
                });
                app.Execute(args);
            }

            using (var webHost = new WebHostBuilder().
                UseUrls($"http://*:{port}").
                UseKestrel().
                UseStartup<Startup>().
                Build())
            {
                Console.WriteLine($"Running on http://*:{port}");
                webHost.Start();
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app) =>
            app.UseOwin(x => x.UseNancy(opt => opt.Bootstrapper = new DefaultNancyBootstrapper()));
    }
}
