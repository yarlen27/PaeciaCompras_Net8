# PaeciaCompras - .NET 8

Sistema de compras empresarial migrado completamente a .NET 8.

## 🚀 Migración Completada

Este repositorio contiene la versión moderna del sistema PaeciaCompras, migrado exitosamente de .NET Framework 3 a .NET 8.

### ✅ Logros de la migración:

- **Framework**: .NET Framework 3 → .NET 8
- **PDF**: iTextSharp → iText7
- **Base de datos**: System.Data.SqlClient → Microsoft.Data.SqlClient  
- **HTTP**: RestSharp modernizado
- **Hosting**: IHostingEnvironment → IWebHostEnvironment
- **Compilación**: 0 errores (de 121 errores originales)

### 🏗️ Arquitectura:

- **API**: ASP.NET Core Web API
- **Core**: Lógica de negocio
- **AspNetCore.Identity.MongoDB**: Autenticación con MongoDB
- **Cobalto.MongoDB.Core**: Acceso a datos MongoDB
- **Cobalto.SQL.Core**: Acceso a datos SQL Server

### 🛠️ Tecnologías:

- .NET 8
- ASP.NET Core
- MongoDB
- SQL Server
- iText7 (PDF)
- RestSharp
- AutoMapper
- NLog

### 📋 Requisitos:

- .NET 8 SDK
- SQL Server
- MongoDB

### ▶️ Ejecución:

```bash
dotnet restore
dotnet build
dotnet run --project API
```

---
🤖 Migrado con [Claude Code](https://claude.ai/code)
