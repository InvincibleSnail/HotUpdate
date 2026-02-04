#!/bin/bash
# 结束占用 8081 的监听进程（本目录 start_server.sh 起的 server）
LISTEN_PID=$(lsof -i :8081 2>/dev/null | awk '/LISTEN/ {print $2}' | head -1)
if [ -n "$LISTEN_PID" ]; then
  echo "结束端口 8081 上的进程 (PID $LISTEN_PID)..."
  kill "$LISTEN_PID" 2>/dev/null
  echo "已结束。"
else
  echo "端口 8081 未被占用。"
fi
