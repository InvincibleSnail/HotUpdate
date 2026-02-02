#!/bin/bash
# 在 Simple 目录下启动静态文件服务器，根目录即本目录（严格隔离）
# 客户端 serverRootUrl 填：http://localhost:8080/
cd "$(dirname "$0")"
echo "热更资源根目录（Simple）: $(pwd)"
echo "客户端 serverRootUrl: http://localhost:8080/"
echo "按 Ctrl+C 停止"
python3 -m http.server 8080
