package blockchain

import (
	"testing"
)

func TestBlockchain(t *testing.T) {
	b := New()
	b.Chain[len(b.Chain)-1].Timestamp = 1562138682.8718169

	// genesis block
	{
		block := b.LastBlock()
		if block.Index != 1 {
			t.Errorf("block index wrong. expected=1, got=%d", block.Index)
		}
		if block.Proof != 100 {
			t.Errorf("block proof wrong. expected=100, got=%d", block.Proof)
		}
		if block.PreviousHash != "1" {
			t.Errorf("block previous hash wrong. expected=1, got=%s", block.PreviousHash)
		}
		if len(block.Transactions) != 0 {
			t.Errorf("block transactions length wrong. expected=0, got=%d", len(block.Transactions))
		}
	}

	// new transaction
	{
		result := b.NewTransaction("8527147fe1f5426f9dd545de4b27ee00", "a77f5cdfa2934df3954a5c7c7da5df1f", 5)
		if result != 2 {
			t.Errorf("new transaction result wrong. expected=2, got=%d", result)
		}
	}

	// proof of work and new block
	{
		proof := ProofOfWork(b.LastBlock())
		previousHash := Hash(b.LastBlock())
		if proof != 2964 {
			t.Errorf("proof wrong. expected=2964, got=%d", proof)
		}
		if previousHash != "26aa6d44bd141589a0d3a1a90b86bf411de90d948abab3557b5b30ca75e4e904" {
			t.Errorf("previous hash wrong. expected=26aa6d44bd141589a0d3a1a90b86bf411de90d948abab3557b5b30ca75e4e904, got=%s", previousHash)
		}

		block := b.NewBlock(proof, previousHash)
		b.Chain[len(b.Chain)-1].Timestamp = 1562138730.8820634
		if block.Index != 2 {
			t.Errorf("block index wrong. expected=2, got=%d", block.Index)
		}
		if block.Proof != proof {
			t.Errorf("block proof wrong. expected=%d, got=%d", proof, block.Proof)
		}
		if block.PreviousHash != previousHash {
			t.Errorf("block previous hash wrong. expected=%s, got=%s", previousHash, block.PreviousHash)
		}

		transactions := block.Transactions
		if len(transactions) != 1 {
			t.Fatalf("transactions length wrong. expected=1, got=%d", len(transactions))
		}
		transaction := transactions[0]
		if transaction.Sender != "8527147fe1f5426f9dd545de4b27ee00" {
			t.Errorf("transaction sender wrong. expected=8527147fe1f5426f9dd545de4b27ee00, got=%s", transaction.Sender)
		}
		if transaction.Recipient != "a77f5cdfa2934df3954a5c7c7da5df1f" {
			t.Errorf("transaction recipient wrong. expected=a77f5cdfa2934df3954a5c7c7da5df1f, got=%s", transaction.Recipient)
		}
		if transaction.Amount != 5 {
			t.Errorf("transaction amount wrong. expected=5, got=%d", transaction.Amount)
		}
	}

	// new transaction
	{
		var result int32
		result = b.NewTransaction("f5904ab1209feccb054a79783a508e66", "ad9be4f7f91188b5b3509efd7e9973fa", 42)
		if result != 3 {
			t.Errorf("new transaction result wrong. expected=3, got=%d", result)
		}
		result = b.NewTransaction("06246b36066a3f615ec81085c89cbefd", "16b539269e260e2cce7bf9a1d666c78e", 13)
		if result != 3 {
			t.Errorf("new transaction result wrong. expected=3, got=%d", result)
		}
	}

	// proof of work and new block
	{
		proof := ProofOfWork(b.LastBlock())
		previousHash := Hash(b.LastBlock())
		if proof != 2409 {
			t.Errorf("proof wrong. expected=2409, got=%d", proof)
		}
		if previousHash != "7ef1a64eec40e553f39acc252bb3444100d6a95f305e74c2fc40d41d86bbc1d4" {
			t.Errorf("previous hash wrong. expected=7ef1a64eec40e553f39acc252bb3444100d6a95f305e74c2fc40d41d86bbc1d4, got=%s", previousHash)
		}

		block := b.NewBlock(proof, previousHash)
		b.Chain[len(b.Chain)-1].Timestamp = 1562138730.8820634
		if block.Index != 3 {
			t.Errorf("block index wrong. expected=3, got=%d", block.Index)
		}
		if block.Proof != proof {
			t.Errorf("block proof wrong. expected=%d, got=%d", proof, block.Proof)
		}
		if block.PreviousHash != previousHash {
			t.Errorf("block previous hash wrong. expected=%s, got=%s", previousHash, block.PreviousHash)
		}

		transactions := block.Transactions
		if len(transactions) != 2 {
			t.Fatalf("transactions length wrong. expected=2, got=%d", len(transactions))
		}
		var transaction *Transaction
		transaction = transactions[0]
		if transaction.Sender != "f5904ab1209feccb054a79783a508e66" {
			t.Errorf("transaction sender wrong. expected=f5904ab1209feccb054a79783a508e66, got=%s", transaction.Sender)
		}
		if transaction.Recipient != "ad9be4f7f91188b5b3509efd7e9973fa" {
			t.Errorf("transaction recipient wrong. expected=ad9be4f7f91188b5b3509efd7e9973fa, got=%s", transaction.Recipient)
		}
		if transaction.Amount != 42 {
			t.Errorf("transaction amount wrong. expected=42, got=%d", transaction.Amount)
		}
		transaction = transactions[1]
		if transaction.Sender != "06246b36066a3f615ec81085c89cbefd" {
			t.Errorf("transaction sender wrong. expected=06246b36066a3f615ec81085c89cbefd, got=%s", transaction.Sender)
		}
		if transaction.Recipient != "16b539269e260e2cce7bf9a1d666c78e" {
			t.Errorf("transaction recipient wrong. expected=16b539269e260e2cce7bf9a1d666c78e, got=%s", transaction.Recipient)
		}
		if transaction.Amount != 13 {
			t.Errorf("transaction amount wrong. expected=13, got=%d", transaction.Amount)
		}
	}

	// proof of work
	{
		proof := ProofOfWork(b.LastBlock())
		previousHash := Hash(b.LastBlock())
		if proof != 35370 {
			t.Errorf("proof wrong. expected=35370, got=%d", proof)
		}
		if previousHash != "a26f1ca57d0cb768cb14e1011afd6166183638e779d7f803b335186531275a31" {
			t.Errorf("previous hash wrong. expected=a26f1ca57d0cb768cb14e1011afd6166183638e779d7f803b335186531275a31, got=%s", previousHash)
		}
	}
}
