#define CATCH_CONFIG_MAIN
#include "Catch2/catch.hpp"
#include "blockchain/blockchain.hpp"

TEST_CASE("blockchain") {
	blockchain::blockchain b;
	b.chain.back().timestamp = 1562138682.871;  // for hash test

	SECTION("genesis block") {
		auto const& block = b.last_block();
		CHECK(block.index == 1);
		CHECK(block.proof == 100);
		CHECK(block.previous_hash == "1");
		CHECK(block.transactions.empty());
	}

	// new transaction
	{
		CHECK(b.new_transaction("8527147fe1f5426f9dd545de4b27ee00", "a77f5cdfa2934df3954a5c7c7da5df1f", 5) == 2);
	}

	// proof of work and new block
	{
		auto const proof = blockchain::blockchain::proof_of_work(b.last_block());
		auto const previious_hash = blockchain::blockchain::hash(b.last_block());
		//CHECK(proof == 46313);
		//CHECK(previious_hash == "c8680050d1ed2a8e26e7be9289d879e97f08b558bd48c1f1bbaa67a9c2b89f7d");
		CHECK(proof == 1549);
		CHECK(previious_hash == "c8680050d1ed2a8e26e7be9289d879e97f08b558bd48c1f1bbaa67a9c2b89f7d");

		auto const& block = b.new_block(proof, previious_hash);
		b.chain.back().timestamp = 1562138730.882;  // for hash test
		CHECK(block.index == 2);
		CHECK(block.proof == proof);
		CHECK(block.previous_hash == previious_hash);
		auto const& transactions = block.transactions;
		REQUIRE(transactions.size() == 1);
		auto const& transaction = transactions[0];
		CHECK(transaction.sender == "8527147fe1f5426f9dd545de4b27ee00");
		CHECK(transaction.recipient == "a77f5cdfa2934df3954a5c7c7da5df1f");
		CHECK(transaction.amount == 5);
	}

	// new transactions
	{
		CHECK(b.new_transaction("f5904ab1209feccb054a79783a508e66", "ad9be4f7f91188b5b3509efd7e9973fa", 42) == 3);
		CHECK(b.new_transaction("06246b36066a3f615ec81085c89cbefd", "16b539269e260e2cce7bf9a1d666c78e", 13) == 3);
	}

	// proof of work and new block
	{
		auto const proof = blockchain::blockchain::proof_of_work(b.last_block());
		auto const previious_hash = blockchain::blockchain::hash(b.last_block());
		//CHECK(proof == 69116);
		//CHECK(previious_hash == "77a98b9263dbf01470580087be8ae4daf1c011f000a7c91674f390da99601d2e");
		CHECK(proof == 5315);
		CHECK(previious_hash == "10025d4d0f391eef9f9e5e4551f823ba4966f427e830125be80d296079f41f6a");

		auto const& block = b.new_block(proof, previious_hash);
		b.chain.back().timestamp = 1562138771.257;  // for hash test
		CHECK(block.index == 3);
		CHECK(block.proof == proof);
		CHECK(block.previous_hash == previious_hash);
		auto const& transactions = block.transactions;
		REQUIRE(transactions.size() == 2);
		{
		auto const& transaction = transactions[0];
		CHECK(transaction.sender == "f5904ab1209feccb054a79783a508e66");
		CHECK(transaction.recipient == "ad9be4f7f91188b5b3509efd7e9973fa");
		CHECK(transaction.amount == 42);
		}
		{
		auto const& transaction = transactions[1];
		CHECK(transaction.sender == "06246b36066a3f615ec81085c89cbefd");
		CHECK(transaction.recipient == "16b539269e260e2cce7bf9a1d666c78e");
		CHECK(transaction.amount == 13);
		}
	}

	// proof of work
	{
		auto const proof = blockchain::blockchain::proof_of_work(b.last_block());
		auto const previious_hash = blockchain::blockchain::hash(b.last_block());
		//CHECK(proof == 160965);
		//CHECK(previious_hash == "b81f7e5fad30a34386d58b96d1424eae06ef9b42572364205e6c99eb724f95a3");
		CHECK(proof == 6818);
		CHECK(previious_hash == "718bd3adbf90d55289047bb17ef78b506b03368b2fc5416f00db1c1bf0b4c708");
	}
}
