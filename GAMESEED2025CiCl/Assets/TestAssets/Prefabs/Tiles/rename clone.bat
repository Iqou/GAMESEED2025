@echo off
setlocal enabledelayedexpansion

REM Mulai dari folder ini dan cari semua file yang mengandung (Clone)
for /R %%F in (*clone*.*) do (
    set "full=%%~fF"
    set "name=%%~nxF"
    set "folder=%%~dpF"

    REM Hapus string "(Clone)" atau "(clone)" dari nama file
    set "cleanName=!name:(Clone)=!"
    set "cleanName=!cleanName:(clone)=!"

    REM Kalau nama berubah, dan file baru belum ada, lakukan rename
    if not "!name!"=="!cleanName!" (
        if not exist "!folder!!cleanName!" (
            echo Renaming: "!name!" → "!cleanName!"
            ren "%%~fF" "!cleanName!"
        ) else (
            echo Skipped: "!folder!!cleanName!" already exists.
        )
    )
)

echo Done.
pause
