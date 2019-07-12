'use strict';

const crypto = require('crypto');
const fetch = require('node-fetch');
const stringify = require('json-stable-stringify');

class Blockchain {
  constructor() {
    this.chain = [];
    this.currentTransactions = [];
    this.nodes = new Set();

    this.newBlock(100, '1');
  }

  registerNode(address) {
    try {
      const parsedUrl = new URL(address);
      if (parsedUrl.host)
        this.nodes.add(parsedUrl.host);
        return
    } catch (_) {}
    try {
      const parsedUrl = new URL(`http://${address}`);
      if (parsedUrl.host)
        this.nodes.add(parsedUrl.host);
        return
    } catch (_) {}
    throw new Error('Invalid URL');
  }

  static validChain(chain) {
    let lastBlock = chain[0];
    let currentIndex = 1;

    while (currentIndex < chain.length) {
      const block = chain[currentIndex];
      console.log(lastBlock);
      console.log(block);
      console.log('\n--------------\n');

      const lastBlockHash = Blockchain.hash(lastBlock);
      if (block['previous_hash'] != lastBlockHash)
        return false;

      if (!Blockchain.validProof(lastBlock['proof'], block['proof'], lastBlockHash))
        return false;

      lastBlock = block;
      currentIndex++;
    }

    return true;
  }

  async resolveConflicts() {
    const neighbors = this.nodes;
    let newChain;

    let maxLength = this.chain.length;

    for (const node of neighbors) {
      try {
        const res = await fetch(`http://${node}/chain`);

        if (res.status == 200) {
          const json = await res.json();
          const length = json['length'];
          const chain = json['chain'];

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

  newBlock(proof, previousHash) {
    const block = {
      'index': this.chain.length + 1,
      'timestamp': Date.now() / 1000,
      'transactions': this.currentTransactions,
      'proof': proof,
      'previous_hash': previousHash || Blockchain.hash(this.chain[this.chain.length - 1]),
    };

    this.currentTransactions = [];

    this.chain.push(block);
    return block;
  }

  newTransaction(sender, recipient, amount) {
    this.currentTransactions.push({
      'sender': sender,
      'recipient': recipient,
      'amount': amount,
    });

    return this.lastBlock['index'] + 1;
  }

  static hash(block) {
    const blockString = stringify(block);
    const hash = crypto.createHash('sha256');
    return hash.update(blockString).digest('hex');
  }

  get lastBlock() {
    return this.chain[this.chain.length - 1];
  }

  static proofOfWork(lastBlock) {
    const lastProof = lastBlock['proof'];
    const lastHash = Blockchain.hash(lastBlock);

    let proof = 0;
    while (!Blockchain.validProof(lastProof, proof, lastHash))
      proof++;

    return proof;
  }

  static validProof(lastProof, proof, lastHash) {
    const guess = `${lastProof}${proof}${lastHash}`;
    const hash = crypto.createHash('sha256');
    const guessHash = hash.update(guess).digest('hex');
    return guessHash.startsWith('0000');
  }
}

module.exports = Blockchain;
