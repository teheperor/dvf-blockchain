#pragma once
#include <cstdint>
#include <chrono>
#include <iostream>
#include <set>
#include <stdexcept>
#include <string>
#include <tuple>
#include <utility>
#include <vector>

#ifndef USE_BOOST_BEAST
#ifdef _WIN32
#include <winsock2.h>
#pragma comment(lib, "WS2_32.LIB")
#endif
#pragma warning(push)
#pragma warning(disable: 4244 4267 4996)
#include "HappyHTTP/happyhttp.h"
#pragma warning(pop)
#else
#define BOOST_DATE_TIME_NO_LIB
#define BOOST_REGEX_NO_LIB
#define BOOST_ERROR_CODE_HEADER_ONLY
#define BOOST_SYSTEM_NO_LIB
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/version.hpp>
#endif

#include "nlohmann/json.hpp"
#pragma warning(push)
#pragma warning(disable: 4244)
#include "PicoSHA2/picosha2.h"
#pragma warning(pop)
#include "url-cpp/url.h"

namespace blockchain {
using size_type = std::vector<nullptr_t>::size_type;

struct transaction {
	transaction() = default;
	transaction(std::string const& sender, std::string const& recipient, int64_t amount) :
		sender{ sender }, recipient{ recipient }, amount{ amount } {}

	std::string sender;
	std::string recipient;
	int64_t amount;
};

struct block {
	block() = default;
	block(
		size_type index, double timestamp,
		std::vector<transaction> const& transactions, int64_t proof, std::string const& previous_hash) :
		index{ index }, timestamp{ timestamp },
		transactions{ transactions }, proof{ proof }, previous_hash{ previous_hash } {}
	block& operator=(block const&) = default;

	size_type index;
	double timestamp;
	std::vector<transaction> transactions;
	int64_t proof;
	std::string previous_hash;
};

void to_json(nlohmann::json& json, transaction const& transaction) {
	json = nlohmann::json{
		{ "sender",  transaction.sender },
		{ "recipient",  transaction.recipient },
		{ "amount",  transaction.amount },
	};
}
void from_json(nlohmann::json const& json, transaction& transaction) {
	transaction.sender = json.at("sender").get<decltype(transaction.sender)>();
	transaction.recipient = json.at("recipient").get<decltype(transaction.recipient)>();
	transaction.amount = json.at("amount").get<decltype(transaction.amount)>();
}
void to_json(nlohmann::json& json, block const& block) {
	json = nlohmann::json{
		{ "index",  block.index },
		{ "timestamp", block.timestamp },
		{ "transactions", block.transactions },
		{ "proof", block.proof },
		{ "previous_hash", block.previous_hash },
	};
}
void from_json(nlohmann::json const& json, block& block) {
	block.index = json.at("index").get<decltype(block.index)>();
	block.timestamp = json.at("timestamp").get<decltype(block.timestamp)>();
	block.transactions = json.at("transactions").get<decltype(block.transactions)>();
	block.proof = json.at("proof").get<decltype(block.proof)>();
	block.previous_hash = json.at("previous_hash").get<decltype(block.previous_hash)>();
}
}

namespace blockchain {
class blockchain {
public:
	blockchain() : current_transactions{}, chain{}, nodes{} {
		new_block(100, "1");
	}

	/*!
	Add a new node to the list of nodes

	@param address Address of node.Eg. 'http://192.168.0.5:5000'
	*/
	void register_node(std::string const& address) {
		auto const addr = address.find("http://", 0) == 0 ? address : "http://" + address;
		Url::Url const parsed_url{ addr };
		if (!parsed_url.host().empty()) {
			auto const node = parsed_url.port() == 0 ?
				parsed_url.host() :
				parsed_url.host() + ":" + std::to_string(parsed_url.port());
			nodes.insert(node);
		}
		else
			throw std::runtime_error("Invalid URL");
	}

	/*!
	Determine if a given blockchain is valid

	@param chain A blockchain
	@return true if valid, false if not
	*/
	static bool valid_chain(std::vector<block>& chain) {
		auto last_block = chain[0];
		size_type current_index = 1;

		while (current_index < chain.size()) {
			auto const& block = chain[current_index];
			std::cout << nlohmann::json{ last_block }.dump() << std::endl;
			std::cout << nlohmann::json{ block }.dump() << std::endl;
			std::cout << std::endl << "--------------" << std::endl;

			auto const last_block_hash = hash(last_block);
			if (block.previous_hash != last_block_hash)
				return false;

			if (!valid_proof(last_block.proof, block.proof, last_block_hash))
				return false;

			last_block = block;
			current_index++;
		}

		return true;
	}

	/*!
	This is our consensus algorithm, it resolves conflicts by replacing our chain with the longest one in the network.

	@return true if our chain was replaced, false if not
	*/
	bool resolve_conflicts() {
		auto neighbors = nodes;
		std::vector<block> new_chain;

		auto max_length = chain.size();

		for (auto const& node : neighbors) {
			auto const response = requests_get("http://" + node + "/chain");

			auto const status_code = std::get<0>(response);
			if (status_code == 200) {
				auto const json = nlohmann::json::parse(std::get<1>(response));
				auto const length = json["length"].get<size_type>();
				std::vector<block> chain = json["chain"];

				if (length > max_length && valid_chain(chain)) {
					max_length = length;
					new_chain = std::move(chain);
				}
			}
		}

		if (!new_chain.empty()) {
			chain = std::move(new_chain);
			return true;
		}

		return false;
	}

	/*!
	Create a new Block in the Blockchain

	@param proof The proof given by the Proof of Work algorithm
	@param previous_hash Hash of previous Block
	@return New Block
	*/
	block const& new_block(int64_t proof, std::string const& previous_hash) {
		chain.emplace_back(
			chain.size() + 1,
			std::chrono::duration_cast<std::chrono::milliseconds>(
				std::chrono::system_clock::now().time_since_epoch()).count() * 1.0e-3,
			std::move(current_transactions),
			proof,
			previous_hash.empty() ? hash(chain.back()) : previous_hash);

		current_transactions = std::vector<transaction>();

		return chain.back();
	}

	/*!
	Creates a new transaction to go into the next mined Block

	@param sender Address of the Sender
	@param recipient Address of the Recipient
	@param amount Amount
	@return The index of the Block that will hold this transaction
	*/
	size_type new_transaction(std::string const& sender, std::string const& recipient, int64_t amount) {
		current_transactions.emplace_back(sender, recipient, amount);

		return last_block().index + 1;
	}

	block const& last_block() const {
		return chain.back();
	}

	/*!
	Creates a SHA-256 hash of a Block

	@param block Block
	*/
	static std::string hash(block const& block) {
		auto const block_string = nlohmann::json{ block }.dump();
		return std::move(picosha2::hash256_hex_string(block_string));
	}

	/*!
	Simple Proof of Work Algorithm

	- Find a number p' such that hash(pp') contains leading 4 zeroes
	- Where p is the previous proof, and p' is the new proof

	@param last_block last Block
	@return
	*/
	static int64_t proof_of_work(block const& last_block) {
		auto const last_proof = last_block.proof;
		auto const last_hash = hash(last_block);

		int64_t proof = 0;
		while (!valid_proof(last_proof, proof, last_hash))
			proof++;

		return proof;
	}

	/*!
	Validates the Proof

	@param last_proof Previous Proof
	@param proof Current Proof
	@param last_hash The hash of the Previous Block
	@return true if correct, false if not.
	*/
	static bool valid_proof(int64_t last_proof, int64_t proof, std::string const& last_hash) {
		auto const guess = std::to_string(last_proof) + std::to_string(proof) + last_hash;
		auto const guess_hash = picosha2::hash256_hex_string(guess);
		//return guess_hash.substr(0, 4) == "0000";
		return guess_hash.substr(0, 3) == "000";
	}

public:
#ifndef USE_BOOST_BEAST
	static std::tuple<unsigned int, std::string> requests_get(std::string const& url) {
		try {
#ifdef _WIN32
			WSAData wsaData;
			auto const startup_result = ::WSAStartup(MAKEWORD(1, 1), &wsaData);
			if (startup_result != 0)
				throw startup_result;
#endif
			Url::Url const parsed_url{ url };

			using result_type = std::tuple<unsigned int, std::string>;
			result_type result;

			auto on_begin = [](happyhttp::Response const* r, void* userdata) {
				auto* result = reinterpret_cast<result_type*>(userdata);
				std::get<0>(*result) = r->getstatus();
			};
			auto on_data = [](happyhttp::Response const* r, void* userdata, unsigned char const* data, int n) {
				auto* result = reinterpret_cast<result_type*>(userdata);
				std::get<1>(*result).append(reinterpret_cast<char const*>(data), n);
			};
			happyhttp::Connection connection{ parsed_url.host().c_str(), parsed_url.port() != 0 ? parsed_url.port() : 80 };
			connection.setcallbacks(on_begin, on_data, nullptr, &result);
			connection.request("GET", parsed_url.path().c_str());
			while (connection.outstanding())
				connection.pump();
#ifdef _WIN32
			::WSACleanup();
#endif

			return result;
		} catch (int e) {
			(e); throw;
		} catch (happyhttp::Wobbly const& e) {
			(e); throw;
		}
	}
#else
	static std::tuple<unsigned int, std::string> requests_get(std::string const& url) {
		namespace asio = boost::asio;
		namespace beast = boost::beast;

		try {
			Url::Url const parsed_url{ url };
	
			asio::io_context ioc;
			asio::ip::tcp::resolver resolver{ ioc };
			asio::ip::tcp::socket socket{ ioc };

			auto const service = parsed_url.port() == 0 ? parsed_url.scheme() : std::to_string(parsed_url.port());
			auto const results = resolver.resolve(parsed_url.host(), service);
			asio::connect(socket, results.begin(), results.end());

			beast::http::request<beast::http::string_body> req{ beast::http::verb::get, parsed_url.path(), 11 };
			req.set(beast::http::field::host, parsed_url.host());
			beast::http::write(socket, req);

			beast::flat_buffer buffer;
			beast::http::response<beast::http::dynamic_body> res;
			beast::http::read(socket, buffer, res);

			boost::system::error_code ec;
			socket.shutdown(asio::ip::tcp::socket::shutdown_both, ec);
			if (ec && ec != boost::system::errc::not_connected)
				throw boost::system::system_error{ ec };

			auto const status_code = res.result_int();
			auto const body_str = beast::buffers_to_string(res.body().data());
			return { status_code, body_str };
		} catch (std::exception const& e) {
			(e); throw;
		}
	}
#endif

public:
	std::vector<transaction> current_transactions;
	std::vector<block> chain;
	std::set<std::string> nodes;
};
}
