#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[][
// ```bash
// #pwd=Ngaq.Serverʹ根目錄
// docker run --rm -p 5000:2341 --name ngaq-server \
// -v "$(pwd)/ExternalRsrc/Ngaq.Server.dev.jsonc":/app/Ngaq.Server.dev.jsonc \
// ngaq-server:1.0 \
// Ngaq.Server.dev.jsonc # 命令行傳參
// ```
// // # 容器名在後


```bash
# 把同一份檔案拷到「純 Linux 檔案系統」
mkdir -p ~/dockercfg
cp Ngaq.Server.dev.jsonc ~/dockercfg/

#若直接從wsl 的/mnt/掛載 則docker容器運行時讀則報無權限

# 改掛這一份
docker run --rm -p 5000:2341 --name ngaq-server \
  -v "$HOME/dockercfg/Ngaq.Server.dev.jsonc":/app/Ngaq.Server.dev.jsonc:ro \
  ngaq-server:1.0 \
  Ngaq.Server.dev.jsonc
```


建庫: E:\_code\CsNgaq\Ngaq.Server\proj\Ngaq.Server.Test\MkDbSchema.cs

]

#H[配置文件][
	Ngaq.Server/ExternalRsrc/Ngaq.Server.dev.jsonc
	
]

#H[啓動程序][
	#H[啓動依賴的docker容器][
		```bash
	cd /mnt/e/_code/CsNgaq/Ngaq.Server
	docker compose up -d
		```
	]
	#H[編譯][
		`sh WebSrv.sh`
	]
	#H[初始化數據庫架構(臨時)][
		```cs
		using Ngaq.Server.Http.Test;

		Console.WriteLine("Hello, World!");

		await MkDbSchema.InitDb([@"E:\_code\CsNgaq\Ngaq.Server\ExternalRsrc\Ngaq.Server.dev.jsonc"]);

		```
		`dotnet run`
	]
	#H[斷點調試運行][
```json
{
			"name": "WebServer",
			"type": "coreclr",
			"request": "launch",
			//"preLaunchTask": "build", // 可根据你的项目配置修改
			"program": "${workspaceFolder}/Ngaq.Server/proj/Ngaq.Server.Http/bin/Debug/net10.0/Ngaq.Server.Http.exe", // 修改为你的可执行文件路径
			"args": [], // 可执行文件的参数
			"cwd": "${workspaceFolder}/Ngaq.Server/proj/Ngaq.Server.Http/bin/Debug/net10.0", // 设置为输出目录
			"console": "internalConsole", // 或 "integratedTerminal" 或 "externalTerminal"
			"stopAtEntry": false // 是否在程序入口处停止
		},
```
	]
	
	#H[嘗試訪問][
		`http://localhost:5000/` (端口號以配置文件爲準)
		
		```json
	{"Data":null,"Errors":[{"Key":"User/AuthenticationFailed","Args":[],"Tags":["BizErr","Public"]}]}
		```
	]
	

]

