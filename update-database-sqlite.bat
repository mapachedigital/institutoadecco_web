@ECHO OFF

REM Copyright (c) 2021, Mapache Digital
REM Version: 1.0
REM Author: Samuel Kobelkowsky
REM Email: samuel@mapachedigital.com
REM 
REM update the database of a .NET proyect using SQLite

REM This batch file requires the MSYS2 (or any linux subsystem) system installed with the rsync command
REM Download installer from https://www.msys2.org/

c:\msys64\usr\bin\env MSYSTEM=MSYS /usr/bin/bash update-database-sqlite.sh %*