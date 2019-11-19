'use strict';

const program = require('commander');
const bodyParser = require('body-parser');
const express = require('express');
const morgan = require('morgan');
const uuidv4 = require('uuid/v4');

const Blockchain = require('./blockchain');

const main = () => {
  program.
    option('-p, --port <n>', 'port to listen on', parseInt).
    parse(process.argv);
  const port = program.port !== undefined ? program.port : 5000;

  const app = express();

  app.use(bodyParser.urlencoded({ extended: true }));
  app.use(bodyParser.json());
  app.use(morgan('short'));

  const nodeIdentifier = uuidv4().replace(/-/g, '');

  const blockchain = new Blockchain();

  app.get('/mine', (_req, res) => {
    const lastBlock = blockchain.lastBlock;
    const proof = Blockchain.proofOfWork(lastBlock);

    blockchain.newTransaction('0', nodeIdentifier, 1);

    const previousHash = Blockchain.hash(lastBlock);
    const block = blockchain.newBlock(proof, previousHash);

    const response = {
      'message': 'New Block Forged',
      'index': block['index'],
      'transactions': block['transactions'],
      'proof': block['proof'],
      'previous_hash': block['previous_hash'],
    };
    res.json(response);
  });

  app.post('/transactions/new', (req, res) => {
    const values = req.body;

    const required = ['sender', 'recipient', 'amount'];
    if (!required.every(x => x in values))
      res.status(400).send('Missing values');

    const index = blockchain.newTransaction(values['sender'], values['recipient'], values['amount']);

    const response = { 'message': `Transaction will be added to Block ${index}` };
    res.status(201).json(response);
  });

  app.get('/chain', (_req, res) => {
    const response = {
      'chain': blockchain.chain,
      'length': blockchain.chain.length,
    };
    res.json(response);
  });

  app.post('/nodes/register', (req, res) => {
    const values = req.body;

    const nodes = values['nodes'];
    if (!nodes)
      res.status(400).send('Error: Please supply a valid list of nodes');

    for (const node of nodes)
      blockchain.registerNode(node);

    const response = {
      'message': 'New nodes have been added',
      'total_nodes': Array.from(blockchain.nodes),
    };
    return res.status(201).json(response);
  });

  app.get('/nodes/resolve', (_req, res, next) => {
    (async () => {
      const replaced = await blockchain.resolveConflicts();

      const response =
        replaced ?
          {
            'message': 'Our chain was replaced',
            'new_chain': blockchain.chain,
          } :
          {
            'message': 'Our chain is authoritative',
            'chain': blockchain.chain,
          };

      return res.json(response);
    })().catch(next);
  });

  app.listen(port, () => console.log(`listening on port ${port}!`));
}

if (require.main === module)
  main();
