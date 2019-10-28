docfx.exe metadata doc\docfx.json
docfx.exe build doc\docfx.json
start docfx.exe serve -p 8080 doc\_site
start http://localhost:8080
