@echo off
cd /d "%~dp0"
if not exist node_modules call npx --yes pnpm@11.28.9 install --frozen-lockfile
call npx --yes pnpm@11.28.9 dev --open
pause
