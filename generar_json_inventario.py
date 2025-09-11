#!/usr/bin/env python3
import pandas as pd
import json
import requests
import os

def obtener_materiales_api():
    """Obtiene la lista de materiales de la API"""
    url = "https://paecia.com:8000/api/Referencia"
    headers = {
        "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqdWFuLmdhdmlyaWEiLCJqdGkiOiIwNjc3ODhjOC1mZTBjLTRjNGMtOTgzOS01MGEzZDU2NzU1ZjciLCJpYXQiOjE3NTY4MTg4NDMsInJvbCI6ImFwaV9hY2Nlc3MiLCJpZCI6IjVjNDY1ZWFjMWU4N2IxMzY0MGZjNDE3MiIsIm5iZiI6MTc1NjgxODg0MywiZXhwIjoxNzU3NTM4ODQzLCJpc3MiOiJ3ZWJBcGkiLCJhdWQiOiJTaWdtYSJ9.KmCZt2ud-VBZMRd8qM0mbXYcjsqKsPSlGw5D_8hPPAg",
        "clientid": "ab907013-8c98-44c8-aa0e-8f1c0d797104"
    }
    
    try:
        response = requests.get(url, headers=headers, verify=False)
        response.raise_for_status()
        return response.json()
    except Exception as e:
        print(f"Error obteniendo materiales de API: {e}")
        return []

def leer_excel_completo():
    """Lee todas las columnas necesarias del Excel"""
    excel_path = "/home/yarlen/projects/PaeciaCompras_Net8/inventario/INVENTARIO MATERIALES GENERAL I.E FEDERICO OZANAM (1).xlsx"
    
    try:
        df = pd.read_excel(excel_path, sheet_name=0)
        return df
    except Exception as e:
        print(f"Error leyendo Excel: {e}")
        return None

def generar_archivos_json():
    """Genera los 297 archivos JSON"""
    
    # Obtener datos
    print("Obteniendo materiales de la API...")
    materiales_api = obtener_materiales_api()
    print(f"Materiales obtenidos de API: {len(materiales_api)}")
    
    print("Leyendo Excel completo...")
    df = leer_excel_completo()
    if df is None:
        return
    
    print(f"Filas en Excel: {len(df)}")
    
    # Crear diccionario de materiales API por nombre
    materiales_dict = {}
    for material in materiales_api:
        nombre = material.get('nombre', '').strip()
        if nombre:
            materiales_dict[nombre] = material
    
    # Crear carpeta si no existe
    output_dir = "/home/yarlen/projects/PaeciaCompras_Net8/Objetos_inventario"
    os.makedirs(output_dir, exist_ok=True)
    
    # Constantes
    ID_PROYECTO_FEDERICO_OZANAM = "da5d1f87-5cc4-43e3-8ee1-743bc970ac71"
    GUID_VACIO = "00000000-0000-0000-0000-000000000000"
    USUARIO_ID = "5c465eac1e87b13640fc4172"
    
    archivos_creados = 0
    errores = 0
    
    # Procesar cada fila
    for index, row in df.iterrows():
        try:
            # Obtener datos de la fila
            nombre_material = str(row['DESCRIPCIÓN ']).strip()
            cantidad = int(row['CANT.'])
            valor_unitario = int(row['PRECIO UNI.'])
            
            # Buscar el ID del material en la API
            if nombre_material not in materiales_dict:
                print(f"Error: Material '{nombre_material}' no encontrado en API")
                errores += 1
                continue
            
            id_insumo = materiales_dict[nombre_material]['id']
            
            # Crear objeto JSON
            objeto_inventario = {
                "numeroPedido": "Carga Masiva",
                "idInsumo": id_insumo,
                "idProyecto": ID_PROYECTO_FEDERICO_OZANAM,
                "idProveedor": GUID_VACIO,
                "valorUnitario": valor_unitario,
                "cantidad": cantidad,
                "factura": 0,
                "observacion": "CARGA MASIVA",
                "usuario": USUARIO_ID
            }
            
            # Crear archivo JSON (fila + 1 porque index empieza en 0)
            numero_fila = index + 1
            archivo_path = os.path.join(output_dir, f"{numero_fila}.json")
            
            with open(archivo_path, 'w', encoding='utf-8') as f:
                json.dump(objeto_inventario, f, indent=2, ensure_ascii=False)
            
            archivos_creados += 1
            
            # Mostrar progreso cada 50 archivos
            if numero_fila % 50 == 0:
                print(f"Procesados {numero_fila} archivos...")
            
        except Exception as e:
            print(f"Error procesando fila {index + 1}: {e}")
            errores += 1
    
    print(f"\n=== RESULTADOS ===")
    print(f"Archivos JSON creados: {archivos_creados}")
    print(f"Errores: {errores}")
    print(f"Total esperado: 297")
    
    # Mostrar algunos ejemplos
    if archivos_creados > 0:
        print(f"\n=== EJEMPLO DEL ARCHIVO 1.json ===")
        try:
            with open(os.path.join(output_dir, "1.json"), 'r', encoding='utf-8') as f:
                ejemplo = json.load(f)
                print(json.dumps(ejemplo, indent=2, ensure_ascii=False))
        except Exception as e:
            print(f"Error mostrando ejemplo: {e}")

if __name__ == "__main__":
    generar_archivos_json()