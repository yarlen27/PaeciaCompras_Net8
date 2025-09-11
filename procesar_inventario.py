#!/usr/bin/env python3
import pandas as pd
import json
import sys

def main():
    # Leer archivo Excel - primera hoja, columna A
    excel_path = "/home/yarlen/projects/PaeciaCompras_Net8/inventario/INVENTARIO MATERIALES GENERAL I.E FEDERICO OZANAM (1).xlsx"
    
    try:
        # Leer la primera hoja del Excel, solo la columna A
        df = pd.read_excel(excel_path, sheet_name=0, usecols=[0])  # Columna A es índice 0
        
        # Obtener el nombre de la primera columna
        column_name = df.columns[0]
        print(f"Columna encontrada: {column_name}")
        
        # Obtener valores de la columna A, excluyendo valores nulos
        materiales_excel = df[column_name].dropna().astype(str).tolist()
        
        print(f"Total de materiales en Excel: {len(materiales_excel)}")
        print("\nPrimeros 10 materiales del Excel:")
        for i, material in enumerate(materiales_excel[:10]):
            print(f"{i+1}: {material}")
            
        # Aquí cargaremos la lista de materiales de la API (paso siguiente)
        return materiales_excel
        
    except Exception as e:
        print(f"Error leyendo Excel: {e}")
        return []

if __name__ == "__main__":
    materiales = main()