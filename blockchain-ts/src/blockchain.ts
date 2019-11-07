import crypto from 'crypto';
import fetch from 'node-fetch';
import humps from 'humps';
import stringify from 'json-stable-stringify';

type Transaction = {
  sender: string;
  recipient: string;
  amount: number;
}

type Block = {
  index: number;
  timestamp: number;
  transactions: Transaction[];
  proof: number;
  previousHash: string;
}

class Blockchain {
  public chain: Block[] = [];
  private currentTransactions: Transaction[] = [];
  public nodes = new Set<string>();

  public constructor() {
    this.newBlock(100, '1');
  }

  /**
   * Add a new node to the list of nodes
   * @param address Address of node. Eg. 'http://192.168.0.5:5000'
   */
  public registerNode(address: string) {
    try {
      if (address.startsWith('https://'))
        address = `http://${address}`
      const parsedUrl = new URL(address);
      if (parsedUrl.host)
        this.nodes.add(parsedUrl.host);
    } catch (_) {
      throw new Error('Invalid URL');
    }
  }

  /**
   * Determine if a given blockchain is valid
   * @param chain A blockchain
   * @return true if valid, false if not
   */
  public static validChain(chain: Block[]) {
    let lastBlock = chain[0];
    let currentIndex = 1;

    while (currentIndex < chain.length) {
      const block = chain[currentIndex];
      console.log(lastBlock);
      console.log(block);
      console.log('\n--------------\n');

      const lastBlockHash = Blockchain.hash(lastBlock);
      if (block.previousHash != lastBlockHash)
        return false;

      if (!Blockchain.validProof(lastBlock.proof, block.proof, lastBlockHash))
        return false;

      lastBlock = block;
      currentIndex++;
    }

    return true;
  }

  /**
   * This is our consensus algorithm, it resolves conflicts by replacing our chain with the longest one in the network.
   * @return true if our chain was replaced, false if not
   */
  public async resolveConflicts() {
    const neighbors = this.nodes;
    let newChain: Block[] | undefined;

    let maxLength = this.chain.length;

    for (const node of neighbors) {
      try {
        const res = await fetch(`http://${node}/chain`);

        if (res.status === 200) {
          const json: any = await res.json();
          const length: number = json['length'];
          const chain = humps.camelizeKeys(json['chain']) as Block[];

          if (length > maxLength && Blockchain.validChain(chain)) {
            maxLength = length;
            newChain = chain;
          }
        }
      } catch (error) {
        console.log(error);
      }
    }

    if (newChain !== undefined) {
      this.chain = newChain;
      return true;
    }

    return false;
  }

  /**
   * Create a new Block in the Blockchain
   * @param proof The proof given by the Proof of Work algorithm
   * @param previousHash Hash of previous Block
   * @return New Block
   */
  public newBlock(proof: number, previousHash: string) {
    const block = {
      index: this.chain.length + 1,
      timestamp: Date.now() / 1000,
      transactions: this.currentTransactions,
      proof: proof,
      previousHash: previousHash || Blockchain.hash(this.chain[this.chain.length - 1]),
    };

    this.currentTransactions = [];

    this.chain.push(block);
    return block;
  }

  /**
   * Creates a new transaction to go into the next mined Block
   * @param sender Address of the Sender
   * @param recipient Address of the Recipient
   * @param amount Amount
   * @return The index of the Block that will hold this transaction
   */
  public newTransaction(sender: string, recipient: string, amount: number) {
    this.currentTransactions.push({
      sender: sender,
      recipient: recipient,
      amount: amount,
    });

    return this.lastBlock.index + 1;
  }

  /**
   * Creates a SHA-256 hash of a Block
   * @param block Block
   */
  public static hash(block: Block) {
    const blockString = stringify(humps.decamelizeKeys(block));
    const hash = crypto.createHash('sha256');
    return hash.update(blockString).digest('hex');
  }

  public get lastBlock() {
    return this.chain[this.chain.length - 1];
  }

  /**
   * Simple Proof of Work Algorithm:
   * - Find a number p' such that hash(pp') contains leading 4 zeroes
   * - Where p is the previous proof, and p' is the new proof
   * @param lastBlock last Block
   * @return
   */
  public static proofOfWork(lastBlock: Block) {
    const lastProof = lastBlock.proof;
    const lastHash = Blockchain.hash(lastBlock);

    let proof = 0;
    while (!Blockchain.validProof(lastProof, proof, lastHash))
      proof++;

    return proof;
  }

  /**
   * Validates the Proof
   * @param lastProof Previous Proof
   * @param proof Current Proof
   * @param lastHash The hash of the Previous Block
   * @return true if correct, false if not.
   */
  public static validProof(lastProof: number, proof: number, lastHash: string) {
    const guess = `${lastProof}${proof}${lastHash}`;
    const hash = crypto.createHash('sha256');
    const guessHash = hash.update(guess).digest('hex');
    return guessHash.startsWith('0000');
  }
}

export { Transaction, Block, Blockchain }
