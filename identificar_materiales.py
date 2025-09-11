#!/usr/bin/env python3
import pandas as pd
import json
import requests

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

def leer_excel():
    """Lee la primera hoja del Excel y retorna la columna A"""
    excel_path = "/home/yarlen/projects/PaeciaCompras_Net8/inventario/INVENTARIO MATERIALES GENERAL I.E FEDERICO OZANAM (1).xlsx"
    
    try:
        # Leer la primera hoja del Excel, solo la columna A
        df = pd.read_excel(excel_path, sheet_name=0, usecols=[0])
        
        # Obtener valores de la columna A, excluyendo valores nulos
        column_name = df.columns[0]
        materiales_excel = df[column_name].dropna().astype(str).tolist()
        
        return materiales_excel
    except Exception as e:
        print(f"Error leyendo Excel: {e}")
        return []

def identificar_materiales():
    """Identifica los IDs de materiales comparando Excel con API"""
    
    # Obtener datos
    print("Obteniendo materiales de la API...")
    materiales_api = obtener_materiales_api()
    print(f"Materiales obtenidos de API: {len(materiales_api)}")
    
    print("Leyendo Excel...")
    materiales_excel = leer_excel()
    print(f"Materiales en Excel: {len(materiales_excel)}")
    
    # Crear diccionario de materiales API por nombre
    materiales_dict = {}
    for material in materiales_api:
        nombre = material.get('nombre', '').strip()
        if nombre:
            materiales_dict[nombre] = material
    
    # Buscar coincidencias exactas
    coincidencias = []
    no_encontrados = []
    
    for material_excel in materiales_excel:
        material_excel = material_excel.strip()
        if material_excel in materiales_dict:
            material_api = materiales_dict[material_excel]
            coincidencias.append({
                'nombre': material_excel,
                'id': material_api['id'],
                'codigo': material_api['codigo'],
                'unidad': material_api['unidad']
            })
        else:
            no_encontrados.append(material_excel)
    
    # Mostrar resultados
    print(f"\n=== RESULTADOS ===")
    print(f"Coincidencias exactas encontradas: {len(coincidencias)}")
    print(f"No encontrados: {len(no_encontrados)}")
    
    print(f"\n=== COINCIDENCIAS (primeras 10) ===")
    for i, match in enumerate(coincidencias[:10]):
        print(f"{i+1}: {match['nombre']}")
        print(f"   ID: {match['id']}")
        print(f"   Código: {match['codigo']}")
        print(f"   Unidad: {match['unidad']}")
        print()
    
    print(f"\n=== NO ENCONTRADOS (primeros 10) ===")
    for i, no_encontrado in enumerate(no_encontrados[:10]):
        print(f"{i+1}: {no_encontrado}")
    
    return coincidencias, no_encontrados

if __name__ == "__main__":
    coincidencias, no_encontrados = identificar_materiales()