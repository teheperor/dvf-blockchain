# dvf-blockchain F#(.NET Core)

[Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)をF#(.NET Core)で写経した。

## Description

VS2019を使った。
[blockchain-netcore](https://github.com/teheperor/dvf-blockchain/tree/master/blockchain-netcore)をF#に書き換えた。  
JSONの扱いには[Chiron](https://github.com/xyncro/chiron)、Webアプリケーションフレームワークには[Suave](https://github.com/SuaveIO/suave)を使用した。どちらもやりたいことを実現するために調べるのにすこし苦労したけど実現できた。  

F#も含めて関数型プログラミング言語は本を読んだことがあるだけだったのでこの程度のものでもいい学習になった。ループを再帰に書き換えたりその中でパターンマッチを使ってリストを処理したりするだけでもなるほど感があった。  

## Dependency

 - [Chiron](https://github.com/xyncro/chiron)
 - [Argu](https://github.com/fsprojects/Argu)
 - [Suave](https://github.com/SuaveIO/suave)
 - [Expecto](https://github.com/haf/expecto)

## License

[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## References
- [Learn Blockchains by Building One \- HackerNoon\.com](https://hackernoon.com/learn-blockchains-by-building-one-117428612f46)
- [dvf/blockchain: A simple Blockchain in Python](https://github.com/dvf/blockchain)
- [ブロックチェ－ンを構築しながら学ぶ \| POSTD](https://postd.cc/learn-blockchains-by-building-one/)
