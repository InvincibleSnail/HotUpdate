@echo off
cd /d "%~dp0"
echo 热更资源根目录（Addressable）: %CD%
echo 客户端 versionJsonUrl: http://localhost:8080/version.json
echo 按 Ctrl+C 停止
python -m http.server 8080
pause
