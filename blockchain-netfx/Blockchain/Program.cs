using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Diagnostics;
using Nancy.Extensions;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
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
            var host = "";
            var port = 0;
            {
                var app = new CommandLineApplication();
                app.HelpOption("-?|--help");
                var optionHost = app.Option<string>("-h|--host <HOST>", "host to listen on", CommandOptionType.SingleValue);
                var optionPort = app.Option<int>("-p|--port <PORT>", "port to listen on", CommandOptionType.SingleValue);
                app.OnExecute(() => {
                    host = optionHost.HasValue() ? optionHost.ParsedValue : "localhost";
                    port = optionPort.HasValue() ? optionPort.ParsedValue : 5000;
                });
                app.Execute(args);
            }

            if (NLog.LogManager.Configuration == null)
            {
                var config = new NLog.Config.LoggingConfiguration();
                var target = new NLog.Targets.ConsoleTarget() { Error = true };
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, target);
                NLog.LogManager.Configuration = config;
            }

            using (var nancyHost = new NancyHost(
                new Uri($"http://{host}:{port}"),
                new Bootstrapper(),
                new HostConfiguration() { UrlReservations = new UrlReservations() { CreateAutomatically = true } }))
            {
                Console.WriteLine($"Running on http://{host}:{port}");
                nancyHost.Start();
                Console.ReadLine();
            }
        }
    }

    public class Bootstrapper: DefaultNancyBootstrapper
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void Configure(INancyEnvironment environment)
        {
            environment.Diagnostics(true, "password");
            environment.Tracing(true, true);
            base.Configure(environment);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                var req = ctx.Request;
                var res = ctx.Response;
                var log = $"{req.UserHostAddress} - {req.Method} {req.Path} {req.ProtocolVersion} {res.StatusCode}";
                logger.Info(log);
            });
        }
    }
}
