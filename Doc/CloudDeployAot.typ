#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngaq.Server 雲服務器部署][
本文記錄 `Ngaq.Server` Web 後端以單一 Native AOT 鏡像部署到雲服務器的流程。
]

#H[1. 目標][
- 只使用一個鏡像: `ngaq-server:aot-slim`
- 不在服務器編譯，僅在本地構建並上傳鏡像 tar 包
- 同一鏡像支持兩種用法
  - 不傳 `migrate` 時，按原邏輯啓動 Web 服務
  - 傳 `migrate` 時，只執行遷移，結束後退出
- 保持鏡像精簡
  - Native AOT
  - 不打包 `wwwroot`
  - 刪除 `.dbg`、`.pdb`
  - 運行層基於 `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled`
]

#H[2. 關鍵文件][
- `Ngaq.Server/Dockerfile-Aot`
- `Ngaq.Server/Deploy/Cloud/docker-compose.aot-slim.yml`
- `Ngaq.Server/ExternalRsrc/Ngaq.Server.cloud.docker.jsonc`
]

#H[3. 本地構建鏡像][
在 Windows PowerShell 中通過 WSL 執行:

```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "cd /mnt/e/_code/CsNgaq && docker build -t ngaq-server:aot-slim -f ./Ngaq.Server/Dockerfile-Aot ."
```

說明:
- `docker build` 的上下文必須是倉庫根 `E:/_code/CsNgaq`
- `Ngaq.Server.Http` 依賴 `Ngaq.Core`、`Ngaq.Backend`、`Tsinswreng.*` 多個相鄰項目
]

#H[4. 導出鏡像][
```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "docker save ngaq-server:aot-slim | gzip -1 > /tmp/ngaq-server-aot-slim.tar.gz"
```
]

#H[5. 上傳到服務器][
先把鏡像與部署文件上傳到服務器:

```bash
scp /tmp/ngaq-server-aot-slim.tar.gz root@<server>:/opt/ngaq-server/upload/
scp Ngaq.Server/Deploy/Cloud/docker-compose.aot-slim.yml root@<server>:/opt/ngaq-server/upload/
scp Ngaq.Server/ExternalRsrc/Ngaq.Server.cloud.docker.jsonc root@<server>:/opt/ngaq-server/upload/
```
]

#H[6. 服務器部署][
在服務器上執行:

```bash
mkdir -p /opt/ngaq-server/deploy /opt/ngaq-server/config
mv /opt/ngaq-server/upload/docker-compose.aot-slim.yml /opt/ngaq-server/deploy/
mv /opt/ngaq-server/upload/Ngaq.Server.cloud.docker.jsonc /opt/ngaq-server/config/
docker load -i /opt/ngaq-server/upload/ngaq-server-aot-slim.tar.gz

cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml up -d postgres redis
docker compose -f docker-compose.aot-slim.yml run --rm web migrate Ngaq.Server.cloud.docker.jsonc
docker compose -f docker-compose.aot-slim.yml up -d web
```
]

#H[7. migrate 參數行爲][
同一鏡像的入口程序爲 `./Ngaq.Server.Http`。

正常啓動:
```bash
./Ngaq.Server.Http Ngaq.Server.cloud.docker.jsonc
```

只跑遷移:
```bash
./Ngaq.Server.Http migrate Ngaq.Server.cloud.docker.jsonc
```

其約定爲:
- 第一個參數是 `migrate` 時，程序執行遷移並退出
- 否則第一個參數視爲配置文件路徑，按原來邏輯啓動 Web
]

#H[8. 驗證][
可用以下命令檢查:

```bash
docker ps
curl -i http://127.0.0.1:2341/Open/Time
```

補充:
- 由於鏡像內已去掉 `wwwroot`，根路徑是否返回前端頁面不再作爲驗證條件
- 以後端 API 可訪問爲準
]
