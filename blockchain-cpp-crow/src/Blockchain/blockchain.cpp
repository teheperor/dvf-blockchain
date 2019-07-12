#define _WIN32_WINNT 0x0A00
#include <cstdint>
#include <algorithm>
#include <string>
#define BOOST_DATE_TIME_NO_LIB
#define BOOST_REGEX_NO_LIB
#define BOOST_ERROR_CODE_HEADER_ONLY
#define BOOST_SYSTEM_NO_LIB
#include <boost/algorithm/string/erase.hpp>
#include <boost/uuid/random_generator.hpp>
#include <boost/uuid/uuid_io.hpp>
#pragma warning(push)
#pragma warning(disable: 4244 4267)
#include "crow/crow.h"
#pragma warning(pop)
#undef BOOST_DATE_TIME_NO_LIB
#undef BOOST_REGEX_NO_LIB
#undef BOOST_ERROR_CODE_HEADER_ONLY
#undef BOOST_SYSTEM_NO_LIB
#include "nlohmann/json.hpp"
#pragma warning(push)
#pragma warning(disable: 4267)
#include "cmdline/cmdline.h"
#pragma warning(pop)
#include "blockchain.hpp"

int main(int argc, char* argv[]) {
	cmdline::parser parser;
	parser.add<int>("port", 'p', "port to listen on", false, 5000, cmdline::range(1, 65535));
	parser.parse_check(argc, argv);
	auto const port = static_cast<uint16_t>(parser.get<int>("port"));

	crow::SimpleApp app;

	boost::uuids::random_generator gen;
	auto const uuid = boost::uuids::to_string(gen());
	auto const node_identifier = boost::algorithm::erase_all_copy(uuid, "-");

	blockchain::blockchain blockchain;

	CROW_ROUTE(app, "/mine").methods("GET"_method)(
		[&blockchain, &node_identifier](crow::request const& req) {
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
		auto res = crow::response(200, response.dump());
		res.add_header("Content-Type", "application/json");
		return res;
	});

	CROW_ROUTE(app, "/transactions/new").methods("POST"_method)(
		[&blockchain](crow::request const& req) {
		auto const values = nlohmann::json::parse(req.body);
		std::string const required[] = { "sender", "recipient", "amount", };
		if (std::any_of(
			std::begin(required), std::end(required),
			[&values](auto& x){ return values.find(x) == values.end(); }))
			return crow::response(400, "Missing values");

		auto const index = blockchain.new_transaction(
			values.at("sender").get<decltype(blockchain::transaction::sender)>(),
			values.at("recipient").get<decltype(blockchain::transaction::recipient)>(),
			values.at("amount").get<decltype(blockchain::transaction::amount)>());

		nlohmann::json const response{
			{ "message", "Transaction will be added to Block " + std::to_string(index) },
		};
		auto res = crow::response(201, response.dump());
		res.add_header("Content-Type", "application/json");
		return res;
	});

	CROW_ROUTE(app, "/chain").methods("GET"_method)(
		[&blockchain](crow::request const& req) {
		nlohmann::json const response{
			{ "chain", blockchain.chain },
			{ "length", blockchain.chain.size() },
		};
		auto res = crow::response(200, response.dump());
		res.add_header("Content-Type", "application/json");
		return res;
	});

	CROW_ROUTE(app, "/nodes/register").methods("POST"_method)(
		[&blockchain](crow::request const& req) {
		auto const values = nlohmann::json::parse(req.body);

		auto const nodes = values.find("nodes");
		if (nodes == values.end())
			return crow::response(400, "Error: Please supply a valid list of nodes");

		for (auto const& node : *nodes)
			blockchain.register_node(node.get<std::string>());

		nlohmann::json const response{
			{ "message", "New nodes have been added" },
			{ "total_nodes", blockchain.nodes },
		};
		auto res = crow::response(201, response.dump());
		res.add_header("Content-Type", "application/json");
		return res;
	});

	CROW_ROUTE(app, "/nodes/resolve").methods("GET"_method)(
		[&blockchain](crow::request const& req) {
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

		auto res = crow::response(200, response.dump());
		res.add_header("Content-Type", "application/json");
		return res;
	});

	app.port(port).multithreaded().run();

	return 1;
}
