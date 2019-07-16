'use strict';

const { parentPort, workerData } = require('worker_threads');

const Blockchain = require('./blockchain');

const proof = Blockchain.proofOfWork(workerData);
parentPort.postMessage(proof);
