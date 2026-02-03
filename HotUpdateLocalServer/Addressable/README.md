# Addressable 热更资源（严格隔离）

本目录即「Addressable 热更」的根目录，与 Simple 对称。包含版本文件、catalog/bundles 与本地测试用 HTTP 服务器脚本。

## 1. 只在 Unity 编辑器里测（不启动 HTTP 服务器）

- 客户端 `versionJsonUrl` 用 **file://** 指向本目录下的 `version.json` 即可。
- 例如：`file:///Users/user/Documents/HotUpdate/HotUpdateLocalServer/Addressable/version.json`
- 注意：file:// 下 LoadContentCatalogAsync 可能受限，建议用 HTTP 测热更。

## 2. 打包后测 / 用真实 HTTP 拉资源

- 在本目录（Addressable）下启动**静态文件服务器**，当前目录即网站根目录。
- 客户端 `versionJsonUrl` 填：`http://localhost:8080/version.json`（本机）或 `http://你的IP:8080/version.json`（同局域网）。

**启动方式：**

- **Mac / Linux**：`./start_server.sh` 或 `python3 -m http.server 8080`（需先 cd 到本目录）
- **Windows**：双击 `start_server.bat` 或 cmd 里 `python -m http.server 8080`（需先 cd 到本目录）

需要本机已安装 Python。

## 目录说明

- `version.json`：版本号 + `catalogUrl`（指向本目录下的 catalog 文件，如 `catalog_v1.json`）。
- **catalog 与 bundles**：由 Unity Addressables **Build Remote Catalog** 生成后，拷贝到本目录即可。
  - 例如：`catalog_v1.json`、`catalog_v1.hash`、以及若干 `*.bundle`。
  - 保证 `version.json` 里的 `catalogUrl` 能访问到对应 catalog（如 `http://localhost:8080/catalog_v1.json`）。

## 与 Simple 的对应关系

| Simple           | Addressable        |
|------------------|--------------------|
| version.json     | version.json       |
| Res/ui_bg.png    | catalog_v1.json + *.bundle |
| start/stop_server| start/stop_server  |
