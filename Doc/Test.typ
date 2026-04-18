#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Ngaq.Server 測試環境與跑測試][
本文只說明 `Ngaq.Server/proj/Ngaq.Server.Test` 的測試環境準備與執行方式。
]

#H[1. 先決條件][
- 已安裝 .NET SDK 10
- 已有可連接的 Postgres
- 已有可連接的 Redis

測試程序集會收編:
- `Ngaq.Core.Test`
- `Ngaq.Backend.Test`
- `Ngaq.Server.Test` 自己的測試
]

#H[2. 測試環境配置文件][
Server 測試使用:
- `Ngaq.Server/proj/Ngaq.Server.Test/Ngaq.Server.test.jsonc`

默認會讀上面的配置文件。你也可以在命令行傳入自定義路徑。
]

#H[3. 開發/測試數據庫隔離要求][
必須保證 `Ngaq.Server.test.jsonc` 的 Postgres 庫名和開發環境不同。

例如:
- 開發環境: `ngaq_server_dev`
- 測試環境: `ngaq_server_test`

當前推薦檢查項:
- `Db.Postgres.Server`
- `Db.Postgres.Port`
- `Db.Postgres.Database`
- `Db.Postgres.UserId`
- `Db.Postgres.Password`
- `Db.Redis.Host`
- `Db.Redis.Port`
]

#H[4. 啓動依賴服務][
在 `Ngaq.Server/` 目錄執行:

```bash
docker compose up -d
```

若你不用 `docker compose`，只要保證 `Ngaq.Server.test.jsonc` 指向的 PG/Redis 可連接即可。
]

#H[4.1 `Test.sh` 的執行環境與工作目錄][
`Ngaq.Server/proj/Ngaq.Server.Test/Test.sh` 是 Bash 腳本，支持:
- Windows 的 Git Bash（推薦）
- Linux 的 Bash

不支持直接在 `cmd.exe` 裏執行；在 PowerShell 請用:

```powershell
bash .\Test.sh
```

腳本自身會計算:
- `SCRIPT_DIR = Ngaq.Server/proj/Ngaq.Server.Test`
- `SERVER_ROOT = Ngaq.Server`

並且在跑 `dotnet run` 前自動 `cd` 到 `SCRIPT_DIR`。  
所以你可以在任意工作目錄啓動它，實際測試工作目錄都會被固定為 `Ngaq.Server.Test`。
]

#H[5. 跑測試][
推薦直接執行腳本（最少手動操作）:

```bash
bash E:/_code/CsNgaq/Ngaq.Server/proj/Ngaq.Server.Test/Test.sh
```

或先進入測試目錄再執行:

```bash
cd E:/_code/CsNgaq/Ngaq.Server/proj/Ngaq.Server.Test
bash Test.sh
```

默認使用:
- `Ngaq.Server/proj/Ngaq.Server.Test/Ngaq.Server.test.jsonc`

也可手動傳入配置文件路徑:

```bash
bash Test.sh /abs/path/to/Ngaq.Server.test.jsonc
```

若你已手動啓動依賴服務，可跳過腳本內的 `docker compose up`:

```bash
bash Test.sh Ngaq.Server.test.jsonc
```

並設置環境變量:

```bash
TEST_SKIP_DOCKER=1 bash Test.sh
```
]

#H[6. 程序會做什麼][
`dotnet run` 後，入口會:
- 讀取 Server 測試配置
- 組裝 DI (`SetupCore + SetupBiz + SetupLocal`)
- 確保 PG 測試庫存在
- 若是空庫則執行 `FullInit` 初始化 schema
- 執行收編後的全部測試樹
]

#H[7. 常見問題][
#H[Postgres/Redis 連不上][
- 先核對 `Ngaq.Server.test.jsonc` 連接信息
- 再核對服務是否啓動
]

#H[誤連到開發庫][
- 立即檢查 `Db.Postgres.Database`
- 確認不是開發庫名
- 建議固定使用 `ngaq_server_test` 這類專用名稱
]

#H[配置文件路徑錯誤][
- 使用 `dotnet run -- <配置文件路徑>` 明確指定
- 路徑建議相對於 `Ngaq.Server.Test` 當前工作目錄
]
]
