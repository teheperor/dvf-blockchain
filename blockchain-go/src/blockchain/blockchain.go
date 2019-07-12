package blockchain

import (
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"net/url"
	"strings"
	"time"
)

type Transaction struct {
	Sender    string `json:"sender"`
	Recipient string `json:"recipient"`
	Amount    int64  `json:"amount"`
}

type Block struct {
	Index        int32          `json:"index"`
	Timestamp    float64        `json:"timestamp"`
	Transactions []*Transaction `json:"transactions"`
	Proof        int64          `json:"proof"`
	PreviousHash string         `json:"previous_hash"`
}

type Blockchain struct {
	currentTransactions []*Transaction
	Chain               []*Block
	Nodes               map[string]bool
}

func New() *Blockchain {
	b := &Blockchain{
		currentTransactions: make([]*Transaction, 0),
		Nodes:               make(map[string]bool),
	}
	b.NewBlock(100, "1")
	return b
}

func (b *Blockchain) RegisterNode(address string) error {
	if parsedUrl, _ := url.Parse(address); parsedUrl.Host != "" {
		b.Nodes[parsedUrl.Host] = true
	} else if parsedUrl, _ = url.Parse("http://" + address); parsedUrl.Host != "" {
		b.Nodes[parsedUrl.Host] = true
	} else {
		return errors.New("Invalid URL")
	}
	return nil
}

func ValidChain(chain []*Block) bool {
	lastBlock := chain[0]
	currentIndex := 1

	for currentIndex < len(chain) {
		block := chain[currentIndex]
		fmt.Println(lastBlock)
		fmt.Println(block)
		fmt.Print("\n-----------\n")

		lastBlockHash := Hash(lastBlock)
		if block.PreviousHash != lastBlockHash {
			return false
		}

		if !ValidProof(lastBlock.Proof, block.Proof, lastBlockHash) {
			return false
		}

		lastBlock = block
		currentIndex++
	}

	return true
}

func (b *Blockchain) ResolveConflicts() bool {
	neighbors := b.Nodes
	var newChain []*Block

	maxLength := len(b.Chain)

	for node, _ := range neighbors {
		response, err := http.Get(fmt.Sprintf("http://%s/chain", node))

		if err == nil && response.StatusCode == 200 {
			defer response.Body.Close()
			var result struct {
				Chain  []*Block `json.chain`
				Length int      `json:length`
			}
			json.NewDecoder(response.Body).Decode(&result)
			length := result.Length
			chain := result.Chain

			if length > maxLength && ValidChain(chain) {
				maxLength = length
				newChain = chain
			}
		}
	}

	if newChain != nil {
		b.Chain = newChain
		return true
	}

	return false
}

func (b *Blockchain) NewBlock(proof int64, previousHash string) *Block {
	block := &Block{
		Index:        int32(len(b.Chain) + 1),
		Timestamp:    float64(time.Now().UnixNano()) * 1.0e-9,
		Transactions: b.currentTransactions,
		Proof:        proof,
		PreviousHash: previousHash,
	}

	b.currentTransactions = make([]*Transaction, 0)

	b.Chain = append(b.Chain, block)
	return block
}

func (b *Blockchain) NewTransaction(sender string, recipient string, amount int64) int32 {
	b.currentTransactions = append(
		b.currentTransactions,
		&Transaction{Sender: sender, Recipient: recipient, Amount: amount})

	return b.LastBlock().Index + 1
}

func (b *Blockchain) LastBlock() *Block {
	return b.Chain[len(b.Chain)-1]
}

func Hash(block *Block) string {
	blockBytes, _ := json.Marshal(block)
	hashBytes := sha256.Sum256(blockBytes)
	return hex.EncodeToString(hashBytes[:])
}

func ProofOfWork(lastBlock *Block) int64 {
	lastProof := lastBlock.Proof
	lastHash := Hash(lastBlock)

	var proof int64 = 0
	for !ValidProof(lastProof, proof, lastHash) {
		proof++
	}

	return proof
}

func ValidProof(lastProof int64, proof int64, lastHash string) bool {
	guess := fmt.Sprintf("%d%d%s", lastProof, proof, lastHash)
	guessHashBytes := sha256.Sum256([]byte(guess))
	guessHash := hex.EncodeToString(guessHashBytes[:])
	return strings.HasPrefix(guessHash, "0000")
}
