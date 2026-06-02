#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngaq.Server 雲端 PG 連接][
本文記錄如何在本機連接部署於雲服務器上的 `Ngaq.Server` PostgreSQL。
]

#H[1. 現狀][
- 雲服務器 IP: `8.136.34.211`
- PG 運行在 Docker 容器 `deploy-postgres-1` 內
- PG 沒有暴露宿主機 `5432`
- 故不能直接連 `8.136.34.211:5432`
]

#H[2. 爲甚麼直連會拒絕][
當前部署採用 `docker-compose.aot-slim.yml`，其中 `postgres` 服務沒有 `ports:` 映射。

因此:
- 外網不能直接訪問服務器的 `5432`
- `Connection to 8.136.34.211:5432 refused` 是符合當前部署的
- 正確做法是走 SSH 隧道
]

#H[3. 本次實測結果][
本次已在本機實測通過:

```text
db=ngaq; user=ngaq
```

本機 TCP 檢查:

```text
127.0.0.1:15432  -> TcpTestSucceeded = True
```

說明:
- 本機 `5432` 已被其他進程佔用
- 故本次隧道使用本機端口 `15432`
]

#H[4. 當前可用連接參數][
當前在本機應使用:

- Host: `127.0.0.1`
- Port: `15432`
- Database: `ngaq`
- User: `ngaq`
- Password: `NgaqDb2026!`

不要使用:

- Host: `8.136.34.211`
- Port: `5432`
]

#H[5. SSH 隧道原理][
本次不是把流量轉到服務器宿主機 `127.0.0.1:5432`，
而是轉到 PG 容器當前內網 IP:

```text
172.18.0.3:5432
```

原因:
- PG 容器沒有映射到宿主機
- 若隧道目標寫成服務器 `127.0.0.1:5432`，本地端口雖會打開，但真正連接數據庫時會失敗
]

#H[6. 命令行連接方法][
先查 PG 容器 IP:

```bash
ssh root@8.136.34.211 "docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' deploy-postgres-1"
```

假設輸出爲 `172.18.0.3`，則開本機隧道:

```bash
ssh -L 15432:172.18.0.3:5432 root@8.136.34.211
```

保持這個 SSH 會話不要關閉，再使用本機客戶端連:

- Host: `127.0.0.1`
- Port: `15432`
]

#H[7. DataGrip 連法][
DataGrip 中數據庫連接填:

- Host: `127.0.0.1`
- Port: `15432`
- Database: `ngaq`
- User: `ngaq`
- Password: `NgaqDb2026!`

有兩種用法:

- 先在外部終端手動開 SSH 隧道，再讓 DataGrip 直連 `127.0.0.1:15432`
- 或在 DataGrip 內自行配置對應 SSH 隧道

但要注意:
- 不能把數據庫主機填成 `8.136.34.211:5432`
- 若使用 DataGrip 內建 SSH 隧道，也應把最終數據庫目標指向 PG 容器內網 IP，而不是服務器 `127.0.0.1:5432`
]

#H[8. 容器重建後的注意事項][
PG 容器內網 IP 可能變化。

若 `deploy-postgres-1` 被重建，應重新執行:

```bash
ssh root@8.136.34.211 "docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' deploy-postgres-1"
```

拿到新 IP 後，再重開 SSH 隧道。
]
