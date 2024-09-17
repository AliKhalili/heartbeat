@echo off
setlocal enabledelayedexpansion

set NAME=%~1
set CONNECTION=%~2
set ADDRESS=%~3

echo Running the command with %CONNECTION% connections to %ADDRESS%...
for /L %%i in (1,1,35) do (
    echo Running iteration %%i...
    bombardier -c %CONNECTION% -m PUT -H "Device-Id:1234" --http1 -a %ADDRESS% -o j > results\%NAME%\result_%CONNECTION%_%%i.json
)

echo %NAME% done!