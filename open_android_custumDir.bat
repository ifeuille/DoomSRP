@echo off


set DIR=%~dp0
set PROJECT_ROOT=%DIR%Client
set dir=J:/software/work/unityhub/versions/2018.4.12f1/Editor/Unity.exe

echo ´ò¿ªandroid°æ±¾  dir:"%dir%"  PROJECT_ROOT:"%PROJECT_ROOT%"


"%dir%" "-projectPath" "%PROJECT_ROOT%" "-buildTarget" "android"
exit



