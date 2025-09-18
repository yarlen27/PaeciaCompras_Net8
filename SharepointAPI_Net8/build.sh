#!/bin/bash

echo "===========================================" 
echo "🚀 Compilando SharepointAPI .NET 8"
echo "==========================================="

echo "🔨 Compilando proyecto..."
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Compilación exitosa!"
    echo ""
    echo "🏃‍♂️ Para ejecutar: dotnet run"
    echo "🌐 URL: https://localhost:7XXX/swagger (el puerto se mostrará al ejecutar)"
else
    echo "❌ Error en la compilación"
    echo "Revisa los errores arriba para más detalles"
    exit 1
fi

echo "==========================================="
echo "✨ Script de compilación completado"
echo "==========================================="