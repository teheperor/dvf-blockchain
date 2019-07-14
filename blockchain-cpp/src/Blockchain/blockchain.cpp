#define _WIN32_WINNT 0x0A00
#include <cstdint>
#include <algorithm>
#include <iostream>
#include <string>
#define BOOST_DATE_TIME_NO_LIB
#define BOOST_REGEX_NO_LIB
#define BOOST_ERROR_CODE_HEADER_ONLY
#define BOOST_SYSTEM_NO_LIB
#include <boost/algorithm/string/erase.hpp>
#include <boost/uuid/random_generator.hpp>
#include <boost/uuid/uuid_io.hpp>
#undef BOOST_DATE_TIME_NO_LIB
#undef BOOST_REGEX_NO_LIB
#undef BOOST_ERROR_CODE_HEADER_ONLY
#undef BOOST_SYSTEM_NO_LIB

#pragma warning(push)
#pragma warning(disable: 4244)
#include "cpp-httplib/httplib.h"
#pragma warning(pop)
#pragma warning(push)
#pragma warning(disable: 4267)
#include "cmdline/cmdline.h"
#pragma warning(pop)
#define ELPP_NO_DEFAULT_LOG_FILE
#include "easyloggingpp/easylogging++.h"
#include "nlohmann/json.hpp"
#include "blockchain.hpp"

INITIALIZE_EASYLOGGINGPP

int main(int argc, char* argv[]) {
	cmdline::parser parser;
	parser.add<int>("port", 'p', "port to listen on", false, 5000, cmdline::range(1, 65535));
	parser.parse_check(argc, argv);
	auto const port = static_cast<uint16_t>(parser.get<int>("port"));

	httplib::Server svr;
	auto *logger = el::Loggers::getLogger("default");
	svr.set_logger([logger](auto const& req, auto const& res) {
		auto const& remote_addr = req.headers.find("REMOTE_ADDR")->second;
		logger->info("%v - %v %v %v %v", remote_addr.c_str(), req.method, req.path, req.version, res.status);
	});

	boost::uuids::random_generator gen;
	auto const uuid = boost::uuids::to_string(gen());
	auto const node_identifier = boost::algorithm::erase_all_copy(uuid, "-");

	blockchain::blockchain blockchain;

	svr.Get("/mine", [&blockchain, &node_identifier](httplib::Request const& req, httplib::Response& res) {
		auto const& last_block = blockchain.last_block();
		auto const proof = blockchain::blockchain::proof_of_work(last_block);

		blockchain.new_transaction("0", node_identifier, 1);

		auto const previous_hash = blockchain::blockchain::hash(last_block);
		auto const& block = blockchain.new_block(proof, previous_hash);

		nlohmann::json const response{
			{ "message", "New Block Forged" },
			{ "index", block.index },
			{ "transactions", block.transactions },
			{ "proof", block.proof },
			{ "previous_hash", block.previous_hash },
		};
		res.set_content(response.dump(), "application/json");
	});

	svr.Post("/transactions/new", [&blockchain](httplib::Request const& req, httplib::Response& res) {
		auto const values = nlohmann::json::parse(req.body);
		std::string const required[] = { "sender", "recipient", "amount", };
		if (std::any_of(
			std::begin(required), std::end(required),
			[&values](auto& x) { return values.find(x) == values.end(); })) {
			res.status = 400;
			res.set_content("Missing values", "text/plain");
			return;
		}

		auto const index = blockchain.new_transaction(
			values.at("sender").get<decltype(blockchain::transaction::sender)>(),
			values.at("recipient").get<decltype(blockchain::transaction::recipient)>(),
			values.at("amount").get<decltype(blockchain::transaction::amount)>());

		nlohmann::json const response{
			{ "message", "Transaction will be added to Block " + std::to_string(index) },
		};
		res.status = 201;
		res.set_content(response.dump(), "application/json");
	});

	svr.Get("/chain", [&blockchain](httplib::Request const& req, httplib::Response& res) {
		nlohmann::json const response{
			{ "chain", blockchain.chain },
			{ "length", blockchain.chain.size() },
		};
		res.set_content(response.dump(), "application/json");
	});

	svr.Post("/nodes/register", [&blockchain](httplib::Request const& req, httplib::Response& res) {
		auto const values = nlohmann::json::parse(req.body);

		auto const nodes = values.find("nodes");
		if (nodes == values.end()) {
			res.status = 400;
			res.set_content("Error: Please supply a valid list of nodes", "text/plain");
			return;
		}

		for (auto const& node : *nodes)
			blockchain.register_node(node.get<std::string>());

		nlohmann::json const response{
			{ "message", "New nodes have been added" },
			{ "total_nodes", blockchain.nodes },
		};
		res.status = 201;
		res.set_content(response.dump(), "application/json");
	});

	svr.Get("/nodes/resolve", [&blockchain](httplib::Request const& req, httplib::Response& res) {
		auto const replaced = blockchain.resolve_conflicts();

		auto const response =
			replaced ?
			nlohmann::json{
				{ "message", "Our chain was replaced" },
				{ "new_chain", blockchain.chain },
			} :
			nlohmann::json{
				{ "message", "Our chain is authoritative" },
				{ "chain", blockchain.chain },
			};

		res.set_content(response.dump(), "application/json");
	});

	std::cout << "Running on http://0.0.0.0:" << port << std::endl;
	svr.listen("0.0.0.0", port);

	return 1;
}
