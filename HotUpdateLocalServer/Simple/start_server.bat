@echo off
cd /d "%~dp0"
echo 热更资源根目录（Simple）: %CD%
echo 客户端 serverRootUrl: http://localhost:8080/
echo 按 Ctrl+C 停止
python -m http.server 8080
pause
