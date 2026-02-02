# Simple 热更资源（严格隔离）

本目录即「Simple 热更」的根目录，包含版本文件、资源与本地测试用 HTTP 服务器脚本。

## 1. 只在 Unity 编辑器里测（不启动 HTTP 服务器）

- 客户端 `serverRootUrl` 用**本地路径**或 **file://** 指向本目录即可。
- 例如（按你电脑实际路径改）：
  - `file:///Users/user/Documents/HotUpdate/HotUpdateLocalServer/Simple/`
  - 或 Windows：`file:///C:/path/to/HotUpdate/HotUpdateLocalServer/Simple/`
- 无需运行任何 server。

## 2. 打包后测 / 用真实 HTTP 拉资源

- 在本目录（Simple）下启动**静态文件服务器**，当前目录即网站根目录。
- 客户端 `serverRootUrl` 填：`http://localhost:8080/`（本机）或 `http://你的IP:8080/`（同局域网）。

**启动方式：**

- **Mac / Linux**：`./start_server.sh` 或 `python3 -m http.server 8080`（需先 cd 到本目录）
- **Windows**：双击 `start_server.bat` 或 cmd 里 `python -m http.server 8080`（需先 cd 到本目录）

需要本机已安装 Python。

## 目录说明

- `version.json`：版本号与文件列表。
- `Res/`：热更资源（如 `ui_bg.png`），客户端通过 `Res/文件名` 访问。
