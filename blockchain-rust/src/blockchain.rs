extern crate reqwest;
use serde::{Deserialize, Serialize};
use serde_json::Value;
use sha2::{Digest, Sha256};
use std::collections::HashSet;
use std::time::SystemTime;
use url::Url;

#[derive(Serialize, Deserialize)]
pub struct Transaction {
    sender: String,
    recipient: String,
    amount: i64,
}

#[derive(Serialize, Deserialize)]
pub struct Block {
    pub index: i32,
    timestamp: f64,
    pub transactions: Vec<Transaction>,
    pub proof: i64,
    pub previous_hash: String,
}

pub struct Blockchain {
    current_transactions: Vec<Transaction>,
    pub chain: Vec<Block>,
    pub nodes: HashSet<String>,
}

impl Blockchain {
    pub fn new() -> Blockchain {
        let mut blockchain = Blockchain {
            current_transactions: Vec::new(),
            chain: Vec::new(),
            nodes: HashSet::new(),
        };

        blockchain.new_block(100, "1");
        blockchain
    }

    /// Add a new node to the list of nodes
    ///
    /// * `address` - Address of node. Eg. 'http://192.168.0.5:5000'
    pub fn register_node(&mut self, address: &str) -> Result<(), String> {
        let url = Url::parse(address).map_err(|_| "Invalid URL".to_string())?;
        match (url.host_str(), url.port()) {
            (Some(host), None) => self.nodes.insert(host.to_string()),
            (Some(host), Some(port)) => self.nodes.insert(format!("{}:{}", host, port)),
            _ => return Err("Invalid URL".to_string()),
        };
        Ok(())
    }

    /// Determine if a given blockchain is valid
    ///
    /// * `chain` - A blockchain
    /// * `return` - true if valid, false if not
    fn valid_chain(chain: &[Block]) -> bool {
        let mut last_block = &chain[0];
        let mut current_index = 1;

        while current_index < chain.len() {
            let block = &chain[current_index];
            println!("{}", serde_json::to_string(last_block).unwrap());
            println!("{}", serde_json::to_string(block).unwrap());
            println!("\n--------------\n");

            let last_block_hash = Blockchain::hash(last_block);
            if block.previous_hash != last_block_hash {
                return false;
            }

            if !Blockchain::valid_proof(last_block.proof, block.proof, &last_block_hash) {
                return false;
            }

            last_block = block;
            current_index += 1;
        }

        true
    }

    /// This is our consensus algorithm, it resolves conflicts
    /// by replacing our chain with the longest one in the network.
    ///
    /// * `return` - true if our chain was replaced, false if not
    pub fn resolve_conflicts(&mut self) -> bool {
        let neighbours = &self.nodes;
        let mut new_chain: Option<Vec<Block>> = None;

        let mut max_length = self.chain.len() as i64;

        for node in neighbours {
            let mut res = reqwest::get(&format!("http://{}/chain", node)).unwrap();

            if res.status() == reqwest::StatusCode::OK {
                let json: Value = serde_json::from_str(&res.text().unwrap()).unwrap();
                let length = json["length"].as_i64().unwrap();
                let chain: Vec<Block> = serde_json::from_value((&json["chain"]).clone()).unwrap();

                if length > max_length && Blockchain::valid_chain(&chain) {
                    max_length = length;
                    new_chain = Some(chain);
                }
            }
        }

        match new_chain {
            Some(chain) => {
                self.chain = chain;
                true
            }
            _ => false,
        }
    }

    /// Create a new Block in the Blockchain
    ///
    /// * `proof` - The proof given by the Proof of Work algorithm
    /// * `previous_hash` - Hash of previous Block
    /// * `return` - New Block
    pub fn new_block(&mut self, proof: i64, previous_hash: &str) -> &Block {
        let block = Block {
            index: self.chain.len() as i32 + 1,
            timestamp: SystemTime::now()
                .duration_since(SystemTime::UNIX_EPOCH)
                .unwrap()
                .as_millis() as f64
                * 1.0e-3,
            transactions: ::std::mem::replace(&mut self.current_transactions, Vec::new()),
            proof,
            previous_hash: if previous_hash.is_empty() {
                Blockchain::hash(self.chain.last().unwrap())
            } else {
                previous_hash.to_string()
            },
        };

        self.chain.push(block);
        self.chain.last().unwrap()
    }

    /// Creates a new transaction to go into the next mined Block
    ///
    /// * `sender` - Address of the Sender
    /// * `recipient` - Address of the Recipient
    /// * `amount` - Amount
    /// * `return` - The index of the Block that will hold this transaction
    pub fn new_transaction(&mut self, sender: &str, recipient: &str, amount: i64) -> i32 {
        self.current_transactions.push(Transaction {
            sender: sender.to_string(),
            recipient: recipient.to_string(),
            amount,
        });
        self.last_block().index + 1
    }

    pub fn last_block(&self) -> &Block {
        self.chain.last().unwrap()
    }

    /// Creates a SHA-256 hash of a Block
    ///
    /// * `block` - Block
    pub fn hash(block: &Block) -> String {
        let block_string = serde_json::to_string(block).unwrap();
        let hash_bytes = Sha256::digest(block_string.as_bytes());
        format!("{:x}", hash_bytes)
    }

    /// Simple Proof of Work Algorithm:
    ///
    /// - Find a number p' such that hash(pp') contains leading 4 zeroes
    /// - Where p is the previous proof, and p' is the new proof
    ///
    /// * `last_block` - last Block
    /// * `return` -
    pub fn proof_of_work(last_block: &Block) -> i64 {
        let last_proof = last_block.proof;
        let last_hash = Blockchain::hash(last_block);

        let mut proof = 0_i64;
        while !Blockchain::valid_proof(last_proof, proof, &last_hash) {
            proof += 1;
        }

        proof
    }

    /// Validates the Proof
    ///
    /// * `last_proof` - Previous Proof
    /// * `proof` - Current Proof
    /// * `last_hash` - The hash of the Previous Block
    /// * `return` - true if correct, false if not.
    fn valid_proof(last_proof: i64, proof: i64, last_hash: &str) -> bool {
        let guess = format!("{}{}{}", last_proof, proof, last_hash);
        let guess_hash_bytes = Sha256::digest(guess.as_bytes());
        let guess_hash = format!("{:x}", guess_hash_bytes);
        guess_hash.starts_with("0000")
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_blockchain() {
        let mut b = Blockchain::new();
        let block = b.chain.last_mut().unwrap();
        block.timestamp = 1562138682.8718169;

        // genesis block
        {
            let block = b.last_block();
            assert_eq!(block.index, 1);
            assert_eq!(block.proof, 100);
            assert_eq!(block.previous_hash, "1");
            assert_eq!(block.transactions.len(), 0);
        }

        // new transaction
        {
            let result = b.new_transaction(
                "8527147fe1f5426f9dd545de4b27ee00",
                "a77f5cdfa2934df3954a5c7c7da5df1f",
                5,
            );
            assert_eq!(result, 2);
        }

        // proof of work and new block
        {
            let proof = Blockchain::proof_of_work(b.last_block());
            let previous_hash = Blockchain::hash(b.last_block());
            assert_eq!(proof, 2964);
            assert_eq!(
                previous_hash,
                "26aa6d44bd141589a0d3a1a90b86bf411de90d948abab3557b5b30ca75e4e904"
            );

            b.new_block(proof, &previous_hash);
            let block = b.chain.last_mut().unwrap();
            block.timestamp = 1562138730.8820634;
            assert_eq!(block.index, 2);
            assert_eq!(block.proof, proof);
            assert_eq!(block.previous_hash, previous_hash);

            let transactions = &block.transactions;
            assert_eq!(transactions.len(), 1);
            let transaction = &transactions[0];
            assert_eq!(transaction.sender, "8527147fe1f5426f9dd545de4b27ee00");
            assert_eq!(transaction.recipient, "a77f5cdfa2934df3954a5c7c7da5df1f");
            assert_eq!(transaction.amount, 5);
        }

        // new transaction
        {
            let result = b.new_transaction(
                "f5904ab1209feccb054a79783a508e66",
                "ad9be4f7f91188b5b3509efd7e9973fa",
                42,
            );
            assert_eq!(result, 3);
            let result = b.new_transaction(
                "06246b36066a3f615ec81085c89cbefd",
                "16b539269e260e2cce7bf9a1d666c78e",
                13,
            );
            assert_eq!(result, 3);
        }

        // proof of work and new block
        {
            let proof = Blockchain::proof_of_work(b.last_block());
            let previous_hash = Blockchain::hash(b.last_block());
            assert_eq!(proof, 2409);
            assert_eq!(
                previous_hash,
                "7ef1a64eec40e553f39acc252bb3444100d6a95f305e74c2fc40d41d86bbc1d4"
            );

            b.new_block(proof, &previous_hash);
            let block = b.chain.last_mut().unwrap();
            block.timestamp = 1562138771.2577565;
            assert_eq!(block.index, 3);
            assert_eq!(block.proof, proof);
            assert_eq!(block.previous_hash, previous_hash);

            let transactions = &block.transactions;
            assert_eq!(transactions.len(), 2);
            let transaction = &transactions[0];
            assert_eq!(transaction.sender, "f5904ab1209feccb054a79783a508e66");
            assert_eq!(transaction.recipient, "ad9be4f7f91188b5b3509efd7e9973fa");
            assert_eq!(transaction.amount, 42);
            let transaction = &transactions[1];
            assert_eq!(transaction.sender, "06246b36066a3f615ec81085c89cbefd");
            assert_eq!(transaction.recipient, "16b539269e260e2cce7bf9a1d666c78e");
            assert_eq!(transaction.amount, 13);
        }

        // proof of work and new block
        {
            let proof = Blockchain::proof_of_work(b.last_block());
            let previous_hash = Blockchain::hash(b.last_block());
            assert_eq!(proof, 28486);
            assert_eq!(
                previous_hash,
                "2d095dd4fb79711fdb3e780f5ae6ddab1190ddfaa170ee1a1dcb2425bd645652"
            );
        }
    }
}
