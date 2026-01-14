# Quick Start Guide

## Prerequisites
1. .NET 10 SDK installed
2. SQL Server with DadABase database accessible
3. Microsoft Foundry Local or LM Studio installed with Phi-4 model

## Setup Steps

### 1. Configure Database Connection
Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=DadABase;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

### 2. Start Phi-4 Model
1. Open Microsoft Foundry Local
2. Select Phi-4 model
3. Click "Load Model"
4. Click "Start Server"
5. Verify it's running on http://localhost:1234

### 3. Run the Application
```bash
cd src/analyzer
dotnet run
```

That's it! The application will:
- Connect to your database
- Process each joke
- Generate image descriptions
- Assign categories
- Display progress
- Show a summary report

## Need Help?
See the full [README.md](README.md) for detailed instructions, troubleshooting, and configuration options.
