# PaeciaCompras Project Information

## Remote Server Paths
- Remote server: 82.197.66.126
- Project location: `/opt/paecia_compras/API/`
- API runs on port 7998

## Database Configuration
- MongoDB: `mongodb://admin:Cobalto.12@127.0.0.1:27017`
- SQL Server: `Data Source=paecia.com,60000;Initial Catalog=PaeciaCompras;User Id=sa;Password=Cobalto.12;TrustServerCertificate=true`

## Working Endpoints
- `/api/Facturas/filtrado` - Fixed with FlexibleDateTimeConverter
- `/api/Proveedor/SagrilafValido` - Working

## Pending Issues
- `/api/Facturas/datosContables` - JSON deserialization issue (InformacionContable class)

## Commands to Run Tests
- `dotnet build` - Build project
- `dotnet run` - Run API directly (avoid Docker cache issues)

## Development Configuration
- Created `appsettings.Development.json` for local development
- Connects to remote MongoDB: `mongodb://admin:Cobalto.12@82.197.66.126:27017`
- Connects to remote SQL Server: `paecia.com,60000`
- API local running on: `http://localhost:7998`

## Test User Credentials
- Username: `juan.gaviria`
- Password: `Compras2022*`
- Use for generating JWT tokens locally