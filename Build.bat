@echo off
setlocal

:: 切換到此批次檔所在目錄
cd /d "%~dp0"

:: 執行 dotnet build
echo [INFO] Building project...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed. DLL not copied.
    pause
    exit /b 1
)

:: 檢查 DLL 是否存在
set "DLL_PATH=bin\Debug\net9.0-windows\BiosReleaseUI.dll"
if not exist "%DLL_PATH%" (
    echo [ERROR] DLL not found: %DLL_PATH%
    pause
    exit /b 1
)

:: 複製 DLL 到當前資料夾
echo [INFO] Copying DLL to current directory...
copy /Y "%DLL_PATH%" ".\BiosReleaseUI.dll" >nul

echo [SUCCESS] DLL copied successfully!
pause
endlocal