#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngaq.Server 雲端運維速查][
本文是針對當前 `Ngaq.Server` 雲端部署場景的常用運維命令速查。
]

#H[1. 當前場景固定信息][
- 雲服務器 IP: `8.136.34.211`
- SSH 用戶: `root`
- 部署目錄: `/opt/ngaq-server/deploy`
- 配置目錄: `/opt/ngaq-server/config`
- Compose 文件: `/opt/ngaq-server/deploy/docker-compose.aot-slim.yml`
- Web 鏡像: `ngaq-server:aot-slim`
- 對外 Web 端口: `2341`
]

#H[2. 先登錄服務器][
```bash
ssh root@8.136.34.211
```
]

#H[3. 查看容器狀態][
進入部署目錄後執行:

```bash
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml ps
```

或直接看 Docker:

```bash
docker ps
```
]

#H[4. 看 Web 實時日誌][
推薦:

```bash
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml logs -f --tail=200 web
```

若直接按容器名看:

```bash
docker logs -f --tail=200 deploy-web-1
```
]

#H[5. 看 PG / Redis 日誌][
```bash
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml logs -f --tail=200 postgres
docker compose -f docker-compose.aot-slim.yml logs -f --tail=200 redis
```
]

#H[6. 重啓服務][
只重啓 Web:

```bash
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml restart web
```

重啓 PG / Redis:

```bash
docker compose -f docker-compose.aot-slim.yml restart postgres redis
```

全部重啓:

```bash
docker compose -f docker-compose.aot-slim.yml restart
```
]

#H[7. 停止與啓動][
停止全部:

```bash
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml down
```

啓動全部:

```bash
docker compose -f docker-compose.aot-slim.yml up -d
```

只保證依賴先起:

```bash
docker compose -f docker-compose.aot-slim.yml up -d postgres redis
```
]

#H[8. 手動執行 migrate][
當需要手動跑遷移時:

```bash
cd /opt/ngaq-server/deploy
docker compose -f docker-compose.aot-slim.yml run --rm web migrate Ngaq.Server.cloud.docker.jsonc
```

說明:
- 這裏用的是同一個 `ngaq-server:aot-slim` 鏡像
- 第一個參數是 `migrate` 時，只執行遷移，結束後退出
]

#H[9. 驗證 Web 是否正常][
在服務器本機驗證:

```bash
curl -i http://127.0.0.1:2341/Open/Time
```

預期:
- 返回 `200 OK`
- 響應體包含 JSON

補充:
- 根路徑 `/` 返回 `404` 屬正常
- 因爲本鏡像刻意不打包 `wwwroot`
]

#H[10. 查 PG 容器內網 IP][
當需要從本機經 SSH 隧道連 PG 時，用這條命令查 PG 容器 IP:

```bash
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' deploy-postgres-1
```

本次部署實測曾得到:

```text
172.18.0.3
```

但這個 IP 在容器重建後可能變。
]

#H[11. 本機連 PG 的 SSH 隧道][
若本機 `5432` 已被佔用，建議用 `15432`:

```bash
ssh -L 15432:<PG容器IP>:5432 root@8.136.34.211
```

例如:

```bash
ssh -L 15432:172.18.0.3:5432 root@8.136.34.211
```

之後本機客戶端使用:

- Host: `127.0.0.1`
- Port: `15432`
- Database: `ngaq`
- User: `ngaq`
- Password: `NgaqDb2026!`
]

#H[12. 一條命令在本機看 Web 日誌][
若不想先手動 SSH 登錄，可在本機直接執行:

```powershell
ssh root@8.136.34.211 "cd /opt/ngaq-server/deploy && docker compose -f docker-compose.aot-slim.yml logs -f --tail=200 web"
```
]

#H[13. 重新部署後的最小檢查順序][
建議按以下順序排查:

1. `docker compose -f docker-compose.aot-slim.yml ps`
2. `docker compose -f docker-compose.aot-slim.yml logs --tail=200 web`
3. `curl -i http://127.0.0.1:2341/Open/Time`
4. 若涉及數據庫，查看 `postgres` 日誌
5. 若本機連 PG 失敗，重新查 PG 容器 IP 再開 SSH 隧道
]
