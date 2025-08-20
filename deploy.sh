#!/bin/bash

# Script de despliegue para PaeciaCompras
# Uso: ./deploy.sh

echo "🚀 Iniciando despliegue de PaeciaCompras..."

# 1. Hacer login en GitHub Container Registry (solo primera vez)
echo "📦 Verificando autenticación con GitHub Registry..."
echo $GITHUB_TOKEN | docker login ghcr.io -u yarlen27 --password-stdin 2>/dev/null || {
    echo "⚠️  No se pudo autenticar. Configura GITHUB_TOKEN o ejecuta:"
    echo "   docker login ghcr.io -u yarlen27"
}

# 2. Obtener la última imagen
echo "⬇️  Descargando última imagen..."
docker pull ghcr.io/yarlen27/paecia-compras-api:latest

# 3. Detener contenedor actual
echo "🛑 Deteniendo contenedor actual..."
docker-compose -f docker-compose.prod.yml down paecia-compras-api

# 4. Iniciar con nueva imagen
echo "▶️  Iniciando nueva versión..."
docker-compose -f docker-compose.prod.yml up -d paecia-compras-api

# 5. Verificar estado
echo "✅ Verificando estado..."
sleep 5
docker ps | grep paecia-compras-api

# 6. Mostrar logs
echo "📋 Últimos logs:"
docker logs --tail 20 paecia-compras-api

echo "✨ Despliegue completado!"
echo "   Ver logs: docker logs -f paecia-compras-api"
echo "   Estado: docker ps"