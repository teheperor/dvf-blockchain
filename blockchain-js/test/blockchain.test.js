'use strict';

const assert = require('power-assert');
const Blockchain = require('../src/blockchain');

describe('', function() {
  it('test blockchain', function() {
    const b = new Blockchain();
    b.chain[b.chain.length - 1]['timestamp'] = 1562138682.871;

    // genesis block
    {
      const block = b.lastBlock;
      assert.equal(block['index'], 1);
      assert.equal(block['proof'], 100);
      assert.equal(block['previous_hash'], '1');
      assert.equal(block['transactions'].length, 0);
    }

    // new transaction
    {
      assert.equal(b.newTransaction('8527147fe1f5426f9dd545de4b27ee00', 'a77f5cdfa2934df3954a5c7c7da5df1f', 5), 2);
    }

    // proof of work and new block
    {
      const proof = Blockchain.proofOfWork(b.lastBlock);
      const previousHash = Blockchain.hash(b.lastBlock);
      assert.equal(proof, 178642);
      assert.equal(previousHash, '4cd44e0e85b6cadad4b2feb0a0ec929d1916c28dc04bfda2a887f7db5605ea4c');

      const block = b.newBlock(proof, previousHash);
      b.chain[b.chain.length - 1]['timestamp'] = 1562138730.882;
      assert.equal(block['index'], 2);
      assert.equal(block['proof'], proof);
      assert.equal(block['previous_hash'], previousHash);

      const transactions = block['transactions'];
      assert.equal(transactions.length, 1);
      const transaction = transactions[0];
      assert.equal(transaction['sender'], '8527147fe1f5426f9dd545de4b27ee00');
      assert.equal(transaction['recipient'], 'a77f5cdfa2934df3954a5c7c7da5df1f');
      assert.equal(transaction['amount'], 5);
    }

    // new transactions
    {
      assert.equal(b.newTransaction('f5904ab1209feccb054a79783a508e66', 'ad9be4f7f91188b5b3509efd7e9973fa', 42), 3);
      assert.equal(b.newTransaction('06246b36066a3f615ec81085c89cbefd', '16b539269e260e2cce7bf9a1d666c78e', 13), 3);
    }

    // proof of work and new block
    {
      const proof = Blockchain.proofOfWork(b.lastBlock);
      const previousHash = Blockchain.hash(b.lastBlock);
      assert.equal(proof, 20194);
      assert.equal(previousHash, 'a915d762a12bd6c5cbcb53eb416a01d14622841fd7fd12b2bd2cd330fea1b954');

      const block = b.newBlock(proof, previousHash);
      b.chain[b.chain.length - 1]['timestamp'] = 1562138771.257;
      assert.equal(block['index'], 3);
      assert.equal(block['proof'], proof);
      assert.equal(block['previous_hash'], previousHash);

      const transactions = block['transactions'];
      assert.equal(transactions.length, 2);
      {
        const transaction = transactions[0];
        assert.equal(transaction['sender'], 'f5904ab1209feccb054a79783a508e66');
        assert.equal(transaction['recipient'], 'ad9be4f7f91188b5b3509efd7e9973fa');
        assert.equal(transaction['amount'], 42);
      }
      {
        const transaction = transactions[1];
        assert.equal(transaction['sender'], '06246b36066a3f615ec81085c89cbefd');
        assert.equal(transaction['recipient'], '16b539269e260e2cce7bf9a1d666c78e');
        assert.equal(transaction['amount'], 13);
      }
    }

    // proof of work
    {
      const proof = Blockchain.proofOfWork(b.lastBlock);
      const previousHash = Blockchain.hash(b.lastBlock);
      assert.equal(proof, 11637);
      assert.equal(previousHash, 'dc31891fcd822b01a209a27015fade818c0178a2a1213b2a5b3d54972c7f1d0e');
    }
  })
});
