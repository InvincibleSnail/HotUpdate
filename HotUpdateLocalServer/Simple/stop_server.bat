@echo off
REM 结束占用 8080 的监听进程（本目录 start_server.bat 起的 server）
for /f "tokens=5" %%a in ('netstat -ano ^| findstr "LISTENING" ^| findstr ":8080"') do (
  echo 结束端口 8080 上的进程 ^(PID %%a^)...
  taskkill /PID %%a /F >nul 2>&1
  echo 已结束。
  goto :done
)
echo 端口 8080 未被占用。
:done
pause
