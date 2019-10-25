#[macro_use] extern crate clap;
#[macro_use] extern crate log;
#[macro_use] extern crate nickel;
use std::env;
use std::io::Read;
use std::sync::Mutex;
use nickel::{Nickel, HttpRouter, Request, Response, MiddlewareResult, MediaType};
use nickel::status::StatusCode;
use serde_json::{json, Value};
use uuid::Uuid;

mod blockchain;
use blockchain::Blockchain;

struct State {
    node_identifier: String,
    blockchain: Mutex<Blockchain>,
}

fn mine<'mw>(req: &mut Request<State>, mut res: Response<'mw, State>) -> MiddlewareResult<'mw, State> {
    let state = req.server_data();
    let node_identifier = &state.node_identifier;
    let mut blockchain = state.blockchain.lock().unwrap();

    blockchain.new_transaction("0", node_identifier, 1);

    let last_block = blockchain.last_block();
    let proof = Blockchain::proof_of_work(last_block);

    let previous_hash = Blockchain::hash(last_block);
    let block = blockchain.new_block(proof, &previous_hash);

    let response = json!({
        "message": "New Block Forged",
        "index": block.index,
        "transactions": block.transactions,
        "proof": block.proof,
        "previous_hash": block.previous_hash,
    });
    res.set(StatusCode::Ok).set(MediaType::Json);
    res.send(response.to_string())
}

fn new_transaction<'mw>(req: &mut Request<State>, mut res: Response<'mw, State>) -> MiddlewareResult<'mw, State> {
    let mut blockchain = req.server_data().blockchain.lock().unwrap();

    let mut body = String::new();
    req.origin.read_to_string(&mut body).unwrap();
    let values: Value = serde_json::from_str(&body).unwrap();
    let required = ["sender", "recipient", "amount"];
    if !required.iter().all(|&k| values.get(k).is_some()) {
        res.set(StatusCode::BadRequest);
        return res.send("Missing values");
    }

    let index = blockchain.new_transaction(
        values["sender"].as_str().unwrap(),
        values["recipient"].as_str().unwrap(),
        values["amount"].as_i64().unwrap()
    );

    let response = json!({
        "message": format!("Transaction will be added to Block {}", index),
    });
    res.set(StatusCode::Created).set(MediaType::Json);
    res.send(response.to_string())
}

fn full_chain<'mw>(req: &mut Request<State>, mut res: Response<'mw, State>) -> MiddlewareResult<'mw, State> {
    let blockchain = req.server_data().blockchain.lock().unwrap();

    let response = json!({
        "chain": &blockchain.chain,
        "length": blockchain.chain.len(),
    });
    res.set(StatusCode::Ok).set(MediaType::Json);
    res.send(response.to_string())
}

fn register_nodes<'mw>(req: &mut Request<State>, mut res: Response<'mw, State>) -> MiddlewareResult<'mw, State> {
    let mut blockchain = req.server_data().blockchain.lock().unwrap();

    let mut body = String::new();
    req.origin.read_to_string(&mut body).unwrap();
    let values: Value = serde_json::from_str(&body).unwrap();

    let nodes = values["nodes"].as_array();
    if nodes.is_none() {
        res.set(StatusCode::BadRequest);
        return res.send("Error: Please supply a valid list of nodes");
    }

    for node in nodes.unwrap() {
        let _ = blockchain.register_node(node.as_str().unwrap());
    }

    let response = json!({
        "message": "New nodes have been added",
        "total_nodes": blockchain.nodes,
    });
    res.set(StatusCode::Created).set(MediaType::Json);
    res.send(response.to_string())
}

fn consensus<'mw>(req: &mut Request<State>, mut res: Response<'mw, State>) -> MiddlewareResult<'mw, State> {
    let mut blockchain = req.server_data().blockchain.lock().unwrap();

    let replaced = blockchain.resolve_conflicts();

    let response = if replaced {
        json!({
            "message": "Our chain was replaced",
            "new_chain": blockchain.chain,
        })
    } else {
        json!({
            "message": "Our chain is authoritative",
            "chain": blockchain.chain,
        })
    };

    res.set(StatusCode::Ok).set(MediaType::Json);
    res.send(response.to_string())
}

fn main() {
    let matches = clap_app!(blockchain =>
        (@arg port: -p --port +takes_value default_value("5000") "port to listen on")
    ).get_matches();
    let port = value_t!(matches, "port", i32).unwrap();

    let node_identifier = Uuid::new_v4().to_simple().to_string();

    let blockchain = Blockchain::new();

    let state = State {
        node_identifier: node_identifier,
        blockchain: Mutex::new(blockchain),
    };

    let mut server = Nickel::with_data(state);

    env::set_var("RUST_LOG", "info");
    env_logger::init();
    // server.utilize(middleware! {|request, response| {
    //     let req = &request.origin;
    //     info!("{} - {} {} {} {}", req.remote_addr, req.method, req.uri, req.version, response.status());
    // }});
    server.utilize(middleware! {|request| {
        let req = &request.origin;
        info!("{} - {} {} {}", req.remote_addr, req.method, req.uri, req.version);
    }});

    server.get("/mine", mine);
    server.post("/transactions/new", new_transaction);
    server.get("/chain", full_chain);
    server.post("/nodes/register", register_nodes);
    server.get("/nodes/resolve", consensus);

    server.listen(format!("127.0.0.1:{}", port)).unwrap();
}
