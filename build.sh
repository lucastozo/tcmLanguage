#!/bin/bash
echo "Building TCM Interpreter for the current platform..."
echo

PROJECT_PATH="./interpreter/interpreter.csproj"

if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    echo "Detected Linux/WSL environment — building Linux (x64)..."
    rm -rf ./dist/linux
    mkdir -p ./dist/linux
    dotnet publish "$PROJECT_PATH" -c Release -r linux-x64 -o "./dist/linux"
    if [ $? -ne 0 ]; then
        echo "ERROR: Linux build failed!"
        echo "Press any key to exit..."
        read -n 1
        exit 1
    fi
	
    mv ./dist/linux/tcmInterpreter ./dist/linux/tcmInterpreter-linux

    echo
    echo "Linux executable: ./dist/linux/tcmInterpreter-linux"
elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" || "$OSTYPE" == "win32" ]]; then
    echo "Detected Windows environment — building Windows (x64)..."
    rm -rf ./dist/windows
    mkdir -p ./dist/windows
    dotnet publish "$PROJECT_PATH" -c Release -r win-x64 -o "./dist/windows"
    if [ $? -ne 0 ]; then
        echo "ERROR: Windows build failed!"
        echo "Press any key to exit..."
        read -n 1
        exit 1
    fi
	
    mv ./dist/windows/tcmInterpreter.exe ./dist/windows/tcmInterpreter-win.exe

    echo
    echo "Windows executable: ./dist/windows/tcmInterpreter-win.exe"
else
    echo "Unsupported OS detected: $OSTYPE"
    exit 1
fi

echo
echo "Build completed successfully!"