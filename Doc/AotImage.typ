#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngaq.Server Native AOT 鏡像][
本文只說明 `Ngaq.Server` 的 Web 後端 Native AOT 鏡像構建與運行。
]

#H[1. 目標][
- 產出 `Ngaq.Server.Http` 的 Linux `x64` Native AOT 原生可執行文件
- 鏡像內直接啓動 `./Ngaq.Server.Http`
- 不走 `dotnet Ngaq.Server.Http.dll` 的 JIT 運行路線
- 運行層基於 `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled`
// 據說 mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled-aot 更小
- 鏡像內不打包前端 `wwwroot`
- 發布後刪除 `.dbg`、`.pdb`
- 本次實測鏡像大小約 `61535235` bytes，約 `58.7 MiB`
]

#H[2. 關鍵文件][
- `Ngaq.Server/Dockerfile-Aot`
- `Ngaq.Server/ExternalRsrc/Ngaq.Server.aot.docker.jsonc`

其中:
- Dockerfile 在倉庫根上下文構建，因爲 `Ngaq.Server.Http` 會引用 `Ngaq.Core`、`Ngaq.Backend` 與多個 `Tsinswreng.*` 本地項目
- 容器專用配置把 PG/Redis 主機名改成 `host.docker.internal`
- 本鏡像只保留後端運行所需內容，不承擔前端靜態站點分發
]

#H[3. 構建鏡像][
在 Windows PowerShell 中經 WSL 執行:

```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "cd /mnt/e/_code/CsNgaq && docker build -t ngaq-server:aot-slim -f ./Ngaq.Server/Dockerfile-Aot ."
```

注意:
- 工作根目錄按業務視角可視爲 `Ngaq.Server/`
- 但 `docker build` 的 `context` 必須是倉庫根 `E:/_code/CsNgaq`
- 否則 Docker 取不到 `Ngaq.Core`、`Ngaq.Backend`、`Tsinswreng.*` 這些相鄰本地項目
]

#H[4. 啓動依賴服務][
先在 WSL 啓動 PG/Redis:

```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "cd /mnt/e/_code/CsNgaq/Ngaq.Server && docker compose up -d"
```

此步只負責拉起:
- `postgres`
- `redis`
]

#H[5. 準備容器配置文件][
由於直接掛載 `/mnt/e/...` 進 Linux 容器時，當前環境曾出現讀取權限問題，建議先拷到 WSL 的純 Linux 文件系統:

```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "mkdir -p ~/dockercfg && cp /mnt/e/_code/CsNgaq/Ngaq.Server/ExternalRsrc/Ngaq.Server.aot.docker.jsonc ~/dockercfg/"
```
]

#H[6. 運行鏡像][
```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "docker run --rm -p 5000:2341 --add-host=host.docker.internal:host-gateway --name ngaq-server-aot -v \$HOME/dockercfg/Ngaq.Server.aot.docker.jsonc:/app/Ngaq.Server.aot.docker.jsonc:ro ngaq-server:aot-slim Ngaq.Server.aot.docker.jsonc"
```

說明:
- 容器內服務端口取配置文件中的 `2341`
- 宿主映射到 `5000`
- `--add-host=host.docker.internal:host-gateway` 讓容器能回連宿主已暴露的 PG/Redis 端口
- 命令行最後一個參數是服務端啓動時讀取的配置文件路徑
]

#H[7. 驗證][
當前實測結果:
- 容器可正常啓動
- Kestrel 正常監聽容器內 `2341`
- 由於本鏡像不再打包 `wwwroot`，它是純後端鏡像，不再提供前端靜態頁面
]

#H[8. 已知前提][
- 本文只解決 Native AOT 鏡像的構建與啓動鏈路
- 若業務接口需要已初始化的 PG schema，仍需按現有方式先建庫建表
- `docker-compose.yml` 中的 `version` 字段已被新版 Compose 視爲過時，但不影響本次鏡像流程
]
