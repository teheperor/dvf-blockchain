# dvf-blockchain Rust

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をRustで写経した。

## Description

Rust 2018、VSCodeを使った。  

unwrap()使いまくっているのはエラー処理はオリジナルのPythonのコードと同程度にしかしないという方針のため。  

Webアプリケーションフレームワークにははじめ[Actix web](https://github.com/actix/actix-web)を使って進めていたけど、Actix webと[reqwest](https://github.com/seanmonstar/reqwest)を組み合わせて使うやり方が分からなかった。  
非同期は標準化やライブラリの対応が落ち着いてから、ということにしてWebアプリケーションフレームワークは[nickel.rs](https://github.com/nickel-org/nickel.rs)に切り替えた。  
今確認したら2019/11/07に[async/awaitがstableになってた](https://blog.rust-lang.org/2019/11/07/Async-await-stable.html)。  

浅くしか触れていないけどゼロコスト抽象化、所有権、借用、ライフタイムあたりにはしびれる。学習コスト高そうだけどもっとやってみたいと思える。Cargoや[creates.io](https://crates.io/)などのエコシステムもいい。  
ビルド時間の長さと生成物のサイズの大きさの改善は将来に期待。  

## Dependency

 - [Serde](https://github.com/serde-rs/serde)
 - [Serde JSON](https://github.com/serde-rs/json)
 - [RustCrypto: hashes](https://github.com/RustCrypto/hashes)
 - [rust-url](https://github.com/servo/rust-url)
 - [reqwest](https://github.com/seanmonstar/reqwest)
 - [clap](https://github.com/clap-rs/clap)
 - [uuid](https://github.com/uuid-rs/uuid)
 - [log](https://github.com/rust-lang/log)
 - [env_logger](https://github.com/sebasmagri/env_logger/)
 - [nickel.rs](https://github.com/nickel-org/nickel.rs)

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
