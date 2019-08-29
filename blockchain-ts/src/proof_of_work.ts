import { parentPort, workerData } from 'worker_threads';

import { Blockchain } from './blockchain';

const proof = Blockchain.proofOfWork(workerData);
parentPort!.postMessage(proof);
