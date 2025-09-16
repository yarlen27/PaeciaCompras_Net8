#!/bin/bash

# Script de despliegue para Windows Server x64
# Uso: ./deploy-windows.sh

echo "===========================================" 
echo "🚀 Iniciando compilación para Windows x64"
echo "==========================================="

# Limpiar compilaciones anteriores
echo "🧹 Limpiando compilaciones anteriores..."
rm -rf ./publish-windows

# Compilar para Windows Server x64
echo "🔨 Compilando para Windows Server x64..."
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish-windows

# Verificar si la compilación fue exitosa
if [ $? -eq 0 ]; then
    echo "✅ Compilación exitosa!"
    echo "📂 Archivos generados en: ./publish-windows"
    echo ""
    echo "📋 Archivos principales generados:"
    ls -la ./publish-windows/API.exe 2>/dev/null && echo "   ✓ API.exe encontrado"
    ls -la ./publish-windows/appsettings.json 2>/dev/null && echo "   ✓ appsettings.json encontrado"
    ls -la ./publish-windows/appsettings.Production.json 2>/dev/null && echo "   ✓ appsettings.Production.json encontrado"
    
    echo ""
    echo "🔧 Para desplegar en Windows Server:"
    echo "   1. Copia la carpeta ./publish-windows al servidor"
    echo "   2. Ejecuta: API.exe"
    echo "   3. La API correrá en el puerto configurado (7998 por defecto)"
    echo ""
    echo "📦 Tamaño total del despliegue:"
    du -sh ./publish-windows
else
    echo "❌ Error en la compilación"
    echo "Revisa los errores arriba para más detalles"
    exit 1
fi

echo "==========================================="
echo "✨ Script de despliegue completado"
echo "==========================================="