echo off

REM Set paths
set "MODROOT=F:\SteamLibrary\steamapps\common\RimWorld\Mods\DirectlyAbandonSettlement\"
set "CURRENTVER=1.6"
set "MODVER=%MODROOT%\%CURRENTVER%"
set "OUTDIR=%MODVER%\Assemblies"


REM Only delete assemblies if they exist
if exist "%OUTDIR%\*.dll" (
    echo Removing existing DLLs...
    del /q "%OUTDIR%\*.dll"
) else (
    echo No existing DLLs to remove.
)

REM Build DLL to output directory
dotnet build .vscode -o "%OUTDIR%"
if %errorlevel% neq 0 (
    echo Build failed. Aborting copy steps.
    exit /b %errorlevel%
)

REM Copy version folders (1.0 to 1.5) if they exist
for %%V in (1.0 1.1 1.2 1.3 1.4 1.5) do (
    if exist "%%V" (
        echo Copying version folder %%V...
        xcopy /E /Y /I "%%V" "%MODROOT%\%%V"
    ) else (
        echo Skipping version folder %%V (not found)
    )
)

REM Copy About folder and its contents (if exists)
if exist About (
    echo Copying About folder...
    xcopy /E /Y /I About "%MODROOT%\About"
)

REM Copy LoadFolders.xml if it exists
if exist LoadFolders.xml (
    echo Copying LoadFolders.xml...
    copy /Y LoadFolders.xml "%MODROOT%\LoadFolders.xml"
) else (
    echo Skipping LoadFolders.xml (not found)
)

REM Optionally copy Languages folder
if exist Languages (
    echo Copying Languages folder...
    xcopy /E /Y /I Languages "%MODVER%\Languages\"
)

REM Optionally copy Textures folder
if exist Textures (
    echo Copying Textures folder...
    xcopy /E /Y /I Textures "%MODVER%\Textures\"
)

REM Optionally copy Defs folder
if exist Defs (
    echo Copying Defs folder...
    xcopy /E /Y /I Defs "%MODVER%\Defs\"
)

REM Only delete assemblies if they exist
if exist "%OUTDIR%\0Harmony.dll" (
    echo Removing existing Harmony DLL...
    del /q "%OUTDIR%\0Harmony.dll"
) else (
    echo No existing Harmony DLL to remove.
)

REM Create ZIP archive of the mod
echo Creating ZIP archive...
set "ZIPNAME=DirectlyAbandonSettlement_%CURRENTVER%.zip"
set "ZIPSOURCE=%MODROOT%"
set "ZIPDEST=%CD%\%ZIPNAME%"

REM Remove existing ZIP if it exists
if exist "%ZIPDEST%" (
    del /q "%ZIPDEST%"
)

powershell -Command "Compress-Archive -Path '%ZIPSOURCE%\*' -DestinationPath '%ZIPDEST%'"

if exist "%ZIPDEST%" (
    echo ZIP created at: %ZIPDEST%
) else (
    echo ZIP creation failed.
)