import program from 'commander';
import bodyParser from 'body-parser';
import express from 'express';
import humps from 'humps';
import morgan from 'morgan';
import uuidv4 from 'uuid/v4';
import { Worker, workerData } from 'worker_threads';

import { Transaction, Block, Blockchain } from './blockchain';

const main = () => {
  program.
    option('-p, --port <n>', 'port to listen on', parseInt).
    parse(process.argv);
  const port: number = program.port !== undefined ? program.port : 5000;

  const app = express();

  app.use(bodyParser.urlencoded({ extended: true }));
  app.use(bodyParser.json());
  app.use(morgan('short'));

  const nodeIdentifier = uuidv4().replace(/-/g, '');

  const blockchain = new Blockchain();

  app.get('/mine', (req, res) => {
    const lastBlock = blockchain.lastBlock;
    const worker = new Worker(`${__dirname}/proof_of_work.js`, { workerData: lastBlock })
    worker.on('message', (msg: number) => {
      const proof = msg;

      blockchain.newTransaction('0', nodeIdentifier, 1);

      const previousHash = Blockchain.hash(lastBlock);
      const block = blockchain.newBlock(proof, previousHash);

      const response = {
        'message': 'New Block Forged',
        'index': block.index,
        'transactions': block.transactions,
        'proof': block.proof,
        'previous_hash': block.previousHash,
      };
      res.json(response);
    });
  });

  app.post('/transactions/new', (req, res) => {
    const values: any = req.body;

    const required = ['sender', 'recipient', 'amount'];
    if (!required.every(x => x in values))
      res.status(400).send('Missing values');

    const index = blockchain.newTransaction(values['sender'], values['recipient'], values['amount']);

    const response = { 'message': `Transaction will be added to Block ${index}` };
    res.status(201).json(response);
  });

  app.get('/chain', (req, res) => {
    const response = {
      'chain': humps.decamelizeKeys(blockchain.chain) as Block[],
      'length': blockchain.chain.length,
    };
    res.json(response);
  });

  app.post('/nodes/register', (req, res) => {
    const values: any = req.body;

    const nodes: string[] = values['nodes'];
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

  app.get('/nodes/resolve', (req, res, next) => {
    (async () => {
      const replaced = await blockchain.resolveConflicts();

      const response: { message: string; new_chain: Block[] } | { message: string; chain: Block[] } =
        replaced ?
          {
            'message': 'Our chain was replaced',
            'new_chain': humps.decamelizeKeys(blockchain.chain) as Block[],
          } :
          {
            'message': 'Our chain is authoritative',
            'chain': humps.decamelizeKeys(blockchain.chain) as Block[],
          };

      return res.json(response);
    })().catch(next);
  });

  app.listen(port, () => console.log(`listening on port ${port}!`));
}

if (require.main === module)
  main();
