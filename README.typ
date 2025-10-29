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
