#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngaq.Server 雲端腳本][
本文記錄當前雲端部署場景下的本地輔助腳本位置與用法。
]

#H[1. 腳本目錄][
腳本存放於:

- `Ngaq.Server/Doc/CloudScripts/`
]

#H[2. 先設環境變量][
這些腳本默認從環境變量取 SSH 參數。

在 WSL shell 中可先執行:

```bash
export NGAQ_HOST=8.136.34.211
export NGAQ_USER=root
export NGAQ_SSH_PASS='TsinswrengAa1'
```
]

#H[3. 常用腳本][
- `cloud-upload-image.sh`
  - 上傳本地 `/tmp/ngaq-server-aot-slim.tar.gz`
- `cloud-upload-all.sh`
  - 上傳鏡像、compose、配置文件
- `cloud-redeploy-web.sh`
  - 在服務器 `docker load` 後重建 `web`
- `cloud-deploy-full.sh`
  - 完整執行 `docker load -> 依賴服務 -> migrate -> web`
- `cloud-tail-web-logs.sh`
  - 查看 `web` 實時日誌
- `cloud-check-web.sh`
  - 查看 `web` 日誌並調 `Open/Time`
- `cloud-curl-web.sh`
  - 依次調 `/Open/Time` 與 `/`
- `cloud-web-logs.sh`
  - 查看 `web` 日誌與 `compose ps`
- `cloud-postgres-check.sh`
  - 查看 PG 日誌並查詢最近用戶
- `cloud-postgres-ip.sh`
  - 查 PG 容器內網 IP
- `cloud-open-pg-tunnel.sh`
  - 在本機開到 PG 容器的 SSH 隧道
]

#H[4. 在 Windows PowerShell 中調用][
若要從 Windows PowerShell 執行，可經 WSL 調用，例如:

```powershell
wsl.exe -d Ubuntu-20.04 -- sh -lc "export NGAQ_HOST=8.136.34.211 NGAQ_USER=root NGAQ_SSH_PASS='TsinswrengAa1'; /mnt/e/_code/CsNgaq/Ngaq.Server/Doc/CloudScripts/cloud-tail-web-logs.sh"
```
]

#H[5. 說明][
- 這些腳本是從本次實際排障、部署流程整理出的可復用版本
- 爲避免把密碼硬編碼進倉庫，腳本統一通過 `NGAQ_SSH_PASS` 取值
- `cloud-open-pg-tunnel.sh` 默認使用本機 `15432`
- PG 容器 IP 在重建後可能變，故開隧道前建議先調 `cloud-postgres-ip.sh`
- 其中 `cloud-upload-all.sh`、`cloud-deploy-full.sh`、`cloud-web-logs.sh`、`cloud-curl-web.sh`、`cloud-postgres-check.sh`
  就是按本次中途臨時腳本的用途整理出的保留版
]
