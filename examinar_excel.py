#!/usr/bin/env python3
import pandas as pd

def examinar_excel():
    excel_path = "/home/yarlen/projects/PaeciaCompras_Net8/inventario/INVENTARIO MATERIALES GENERAL I.E FEDERICO OZANAM (1).xlsx"
    
    try:
        # Leer todas las columnas de la primera hoja
        df = pd.read_excel(excel_path, sheet_name=0)
        
        print("=== ESTRUCTURA DEL EXCEL ===")
        print(f"Filas: {len(df)}")
        print(f"Columnas: {len(df.columns)}")
        print("\n=== NOMBRES DE COLUMNAS ===")
        for i, col in enumerate(df.columns):
            print(f"{i}: {col}")
        
        print("\n=== PRIMERAS 5 FILAS ===")
        print(df.head())
        
        print(f"\n=== TIPOS DE DATOS ===")
        print(df.dtypes)
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    examinar_excel()