#!/usr/bin/env python3
import json
import requests
import os
import time

def ejecutar_carga_masiva():
    """Ejecuta POST para cada archivo JSON"""
    
    # Configuración
    url = "https://paecia.com:8000/api/MovimientoInventario/IngresarSinOC"
    headers = {
        "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqdWFuLmdhdmlyaWEiLCJqdGkiOiIwNjc3ODhjOC1mZTBjLTRjNGMtOTgzOS01MGEzZDU2NzU1ZjciLCJpYXQiOjE3NTY4MTg4NDMsInJvbCI6ImFwaV9hY2Nlc3MiLCJpZCI6IjVjNDY1ZWFjMWU4N2IxMzY0MGZjNDE3MiIsIm5iZiI6MTc1NjgxODg0MywiZXhwIjoxNzU3NTM4ODQzLCJpc3MiOiJ3ZWJBcGkiLCJhdWQiOiJTaWdtYSJ9.KmCZt2ud-VBZMRd8qM0mbXYcjsqKsPSlGw5D_8hPPAg",
        "clientid": "ab907013-8c98-44c8-aa0e-8f1c0d797104",
        "Content-Type": "application/json"
    }
    
    objetos_dir = "/home/yarlen/projects/PaeciaCompras_Net8/Objetos_inventario"
    
    # Contadores
    exitosos = 0
    errores = 0
    errores_detalle = []
    
    print("=== INICIANDO CARGA MASIVA ===")
    print(f"Endpoint: {url}")
    print(f"Archivos a procesar: 297")
    print()
    
    # Procesar cada archivo del 1 al 297
    for numero_archivo in range(1, 298):
        archivo_path = os.path.join(objetos_dir, f"{numero_archivo}.json")
        
        try:
            # Leer archivo JSON
            with open(archivo_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            
            # Ejecutar POST
            response = requests.post(url, json=data, headers=headers, verify=False)
            
            if response.status_code == 200 or response.status_code == 201:
                exitosos += 1
                if numero_archivo % 25 == 0:  # Mostrar progreso cada 25
                    print(f"✅ Procesados {numero_archivo}/297 archivos - Exitosos: {exitosos}, Errores: {errores}")
            else:
                errores += 1
                error_info = {
                    'archivo': numero_archivo,
                    'status_code': response.status_code,
                    'response': response.text[:200] if response.text else 'Sin respuesta'
                }
                errores_detalle.append(error_info)
                print(f"❌ Error en archivo {numero_archivo}: HTTP {response.status_code}")
                print(f"   Respuesta: {response.text[:100]}...")
            
            # Pequeña pausa para no sobrecargar el servidor
            time.sleep(0.1)
            
        except FileNotFoundError:
            errores += 1
            errores_detalle.append({
                'archivo': numero_archivo,
                'error': 'Archivo no encontrado'
            })
            print(f"❌ Archivo {numero_archivo}.json no encontrado")
            
        except Exception as e:
            errores += 1
            errores_detalle.append({
                'archivo': numero_archivo,
                'error': str(e)
            })
            print(f"❌ Error procesando archivo {numero_archivo}: {e}")
    
    print("\n=== RESUMEN FINAL ===")
    print(f"Total procesados: {exitosos + errores}")
    print(f"✅ Exitosos: {exitosos}")
    print(f"❌ Errores: {errores}")
    print(f"📊 Tasa de éxito: {(exitosos/(exitosos+errores)*100):.1f}%")
    
    if errores > 0:
        print(f"\n=== DETALLES DE ERRORES (primeros 10) ===")
        for error in errores_detalle[:10]:
            print(f"Archivo {error['archivo']}: {error.get('status_code', 'N/A')} - {error.get('response', error.get('error', 'Error desconocido'))}")
    
    # Guardar log de errores si hay errores
    if errores_detalle:
        with open('/home/yarlen/projects/PaeciaCompras_Net8/errores_carga_masiva.json', 'w', encoding='utf-8') as f:
            json.dump(errores_detalle, f, indent=2, ensure_ascii=False)
        print(f"\n📝 Log de errores guardado en: errores_carga_masiva.json")

if __name__ == "__main__":
    ejecutar_carga_masiva()