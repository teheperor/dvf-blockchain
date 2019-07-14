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

#pragma warning(push)
#pragma warning(disable: 4244)
#include "cpp-httplib/httplib.h"
#pragma warning(pop)
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

	bool resolve_conflicts() {
		auto neighbors = nodes;
		std::vector<block> new_chain;

		auto max_length = chain.size();

		for (auto const& node : neighbors) {
			Url::Url const url("http://" + node);
			httplib::Client cli(url.host().c_str(), url.port() >= 0? url.port(): 80);
			auto const res = cli.Get("/chain");

			if (res && res->status == 200) {
				auto const json = nlohmann::json::parse(res->body);
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

	size_type new_transaction(std::string const& sender, std::string const& recipient, int64_t amount) {
		current_transactions.emplace_back(sender, recipient, amount);

		return last_block().index + 1;
	}

	block const& last_block() const {
		return chain.back();
	}

	static std::string hash(block const& block) {
		auto const block_string = nlohmann::json{ block }.dump();
		return std::move(picosha2::hash256_hex_string(block_string));
	}

	static int64_t proof_of_work(block const& last_block) {
		auto const last_proof = last_block.proof;
		auto const last_hash = hash(last_block);

		int64_t proof = 0;
		while (!valid_proof(last_proof, proof, last_hash))
			proof++;

		return proof;
	}

	static bool valid_proof(int64_t last_proof, int64_t proof, std::string const& last_hash) {
		auto const guess = std::to_string(last_proof) + std::to_string(proof) + last_hash;
		auto const guess_hash = picosha2::hash256_hex_string(guess);
		//return guess_hash.substr(0, 4) == "0000";
		return guess_hash.substr(0, 3) == "000";
	}

public:
	std::vector<transaction> current_transactions;
	std::vector<block> chain;
	std::set<std::string> nodes;
};
}
