import json
import unittest
from urllib.parse import urlparse
import urllib.request

class BlockchainTest(unittest.TestCase):
    def __init__(self, server1, server2):
        super().__init__('run_test')
        self.server1 = server1
        self.server2 = server2
        self.values = {}

    def run_test(self):
        tests = [
            self.test_server1_chain_1st,
            self.test_server1_mine_1st,
            self.test_server1_chain_2nd,
            self.test_server1_transactions_new_1st,
            self.test_server1_mine_2nd,
            self.test_server1_chain_3rd,

            self.test_server2_chain_1st,
            self.test_server2_mine_1st,
            self.test_server2_chain_2nd,

            self.test_server1_nodes_register_1st,
            self.test_server1_nodes_resolve_1st,

            self.test_server2_nodes_register_1st,
            self.test_server2_nodes_resolve_1st,
        ]
        for test in tests:
            with self.subTest(test=test):
                test()

    def test_server1_chain_1st(self):
        req = urllib.request.Request(f'{server1}/chain')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            block = obj['chain'][0]
            self.assertEqual(block['index'], 1)
            self.assertEqual(block['previous_hash'], '1')
            self.assertEqual(block['proof'], 100)
            self.assertEqual(len(block['transactions']), 0)

            self.values['server1-block-1'] = block

    def test_server1_mine_1st(self):
        req = urllib.request.Request(f'{server1}/mine')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'New Block Forged')

            transactions = obj['transactions']
            self.assertEqual(len(transactions), 1)
            transaction = transactions[0]
            self.assertEqual(transaction['amount'], 1)
            self.assertEqual(transaction['sender'], '0')

            self.values['server1-mine-1'] = obj
            self.values['server1-node-identifier'] = transaction['recipient']

    def test_server1_chain_2nd(self):
        req = urllib.request.Request(f'{server1}/chain')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            chain = obj['chain']
            self.assertEqual(len(chain), 2)

            self.assertEqual(chain[0], self.values['server1-block-1'])

            block = obj['chain'][1]
            mine = self.values['server1-mine-1']
            self.assertEqual(block['index'], mine['index'])
            self.assertEqual(block['previous_hash'], mine['previous_hash'])
            self.assertEqual(block['proof'], mine['proof'])
            self.assertEqual(block['transactions'], mine['transactions'])

            self.values['server1-block-2'] = block

    def test_server1_transactions_new_1st(self):
        data = {
            'sender': self.values['server1-node-identifier'],
            'recipient': 'someone-other-address',
            'amount': 5,
        }
        headers = { 'Content-Type': 'application/json' }
        req = urllib.request.Request(
            f'{server1}/transactions/new', json.dumps(data).encode(), headers)
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 201)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'Transaction will be added to Block 3')

    def test_server1_mine_2nd(self):
        req = urllib.request.Request(f'{server1}/mine')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'New Block Forged')

            transactions = obj['transactions']
            self.assertEqual(len(transactions), 2)
            node = self.values['server1-node-identifier']

            transaction = transactions[0]
            self.assertEqual(transaction['amount'], 5)
            self.assertEqual(transaction['recipient'], 'someone-other-address')
            self.assertEqual(transaction['sender'], node)

            transaction = transactions[1]
            self.assertEqual(transaction['amount'], 1)
            self.assertEqual(transaction['recipient'], node)
            self.assertEqual(transaction['sender'], '0')

            self.values['server1-mine-2'] = obj

    def test_server1_chain_3rd(self):
        req = urllib.request.Request(f'{server1}/chain')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            chain = obj['chain']
            self.assertEqual(len(chain), 3)

            self.assertEqual(chain[0], self.values['server1-block-1'])
            self.assertEqual(chain[1], self.values['server1-block-2'])

            block = obj['chain'][2]
            mine = self.values['server1-mine-2']
            self.assertEqual(block['index'], mine['index'])
            self.assertEqual(block['previous_hash'], mine['previous_hash'])
            self.assertEqual(block['proof'], mine['proof'])
            self.assertEqual(block['transactions'], mine['transactions'])

            self.values['server1-block-3'] = block

    def test_server2_chain_1st(self):
        req = urllib.request.Request(f'{server2}/chain')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            block = obj['chain'][0]
            self.assertEqual(block['index'], 1)
            self.assertEqual(block['previous_hash'], '1')
            self.assertEqual(block['proof'], 100)
            self.assertEqual(len(block['transactions']), 0)

            self.values['server2-block-1'] = block

    def test_server2_mine_1st(self):
        req = urllib.request.Request(f'{server2}/mine')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'New Block Forged')

            transactions = obj['transactions']
            self.assertEqual(len(transactions), 1)
            transaction = transactions[0]
            self.assertEqual(transaction['amount'], 1)
            self.assertEqual(transaction['sender'], '0')

            self.values['server2-mine-1'] = obj
            self.values['server2-node-identifier'] = transaction['recipient']

    def test_server2_chain_2nd(self):
        req = urllib.request.Request(f'{server2}/chain')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            chain = obj['chain']
            self.assertEqual(len(chain), 2)

            self.assertEqual(chain[0], self.values['server2-block-1'])

            block = obj['chain'][1]
            mine = self.values['server2-mine-1']
            self.assertEqual(block['index'], mine['index'])
            self.assertEqual(block['previous_hash'], mine['previous_hash'])
            self.assertEqual(block['proof'], mine['proof'])
            self.assertEqual(block['transactions'], mine['transactions'])

            self.values['server2-block-2'] = block

    def test_server1_nodes_register_1st(self):
        data = { 'nodes': [ server2 ] }
        headers = { 'Content-Type': 'application/json' }
        req = urllib.request.Request(
            f'{server1}/nodes/register', json.dumps(data).encode(), headers)
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 201)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'New nodes have been added')
            nodes = obj['total_nodes']
            self.assertEqual(len(nodes), 1)
            self.assertEqual(nodes[0], urlparse(self.server2).netloc)

    def test_server1_nodes_resolve_1st(self):
        req = urllib.request.Request(f'{self.server1}/nodes/resolve')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'Our chain is authoritative')

            chain = obj['chain']
            self.assertEqual(len(chain), 3)
            self.assertEqual(chain[0], self.values['server1-block-1'])
            self.assertEqual(chain[1], self.values['server1-block-2'])
            self.assertEqual(chain[2], self.values['server1-block-3'])

    def test_server2_nodes_register_1st(self):
        data = { 'nodes': [ self.server1 ] }
        headers = { 'Content-Type': 'application/json' }
        req = urllib.request.Request(
            f'{server2}/nodes/register', json.dumps(data).encode(), headers)
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 201)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'New nodes have been added')
            nodes = obj['total_nodes']
            self.assertEqual(len(nodes), 1)
            self.assertEqual(nodes[0], urlparse(self.server1).netloc)

    def test_server2_nodes_resolve_1st(self):
        req = urllib.request.Request(f'{self.server2}/nodes/resolve')
        with urllib.request.urlopen(req) as res:
            body = res.read()

            self.assertEqual(res.status, 200)
            self.assertIn('application/json', res.getheader('Content-Type'))

            obj = json.loads(body)
            self.assertEqual(obj['message'], 'Our chain was replaced')

            chain = obj['new_chain']
            self.assertEqual(len(chain), 3)
            self.assertEqual(chain[0], self.values['server1-block-1'])
            self.assertEqual(chain[1], self.values['server1-block-2'])
            self.assertEqual(chain[2], self.values['server1-block-3'])

if __name__ == '__main__':
    from argparse import ArgumentParser
    parser = ArgumentParser()
    parser.add_argument('-s1', '--server1', default="http://localhost:5000", type=str)
    parser.add_argument('-s2', '--server2', default="http://localhost:5001", type=str)
    args = parser.parse_args()
    server1 = args.server1
    server2 = args.server2

    suite = unittest.TestSuite()
    suite.addTest(BlockchainTest(server1, server2))
    unittest.TextTestRunner().run(suite)
