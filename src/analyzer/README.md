# Joke Analyzer - Batch Processing Application

A .NET 10 console application that processes jokes in batch using the Phi-4 local AI model. The application generates image descriptions for each joke and automatically categorizes them.

## Overview

This application:

- Reads jokes from the DadABase SQL database
- Generates image descriptions for each joke using Phi-4
- Automatically categorizes jokes based on their content
- Creates new categories when appropriate
- Updates the database with generated descriptions and categories
- Provides real-time progress updates during processing

## Prerequisites

- .NET 10 SDK or later
- SQL Server with the DadABase database
- Microsoft Foundry Local (formerly LM Studio) or compatible OpenAI-compatible local server
- Phi-4 model downloaded and running locally

## Installing Phi-4 Model

### Option 1: Using Microsoft Foundry Local (Recommended)

1. **Install Microsoft Foundry Local**
   - See: [Running Phi-4 locally with Microsoft Foundry Local: A step-by-step guide](https://techcommunity.microsoft.com/blog/educatordeveloperblog/running-phi-4-locally-with-microsoft-foundry-local-a-step-by-step-guide/4466304)
   - Visit: [https://learn.microsoft.com/en-us/azure/ai-foundry/foundry-local/get-started](https://learn.microsoft.com/en-us/azure/ai-foundry/foundry-local/get-started)
   - Download and install Microsoft Foundry Local for your operating system and follow the installation wizard

   - For a Windows system command-line install:

      ```bash
      winget install Microsoft.FoundryLocal
      foundry --version
      foundry model download phi-4-mini-reasoning
      foundry cache ls
      foundry model run phi-4-mini-reasoning
      foundry service start
      ```

      The service start will return a message like `Service is already running on http://127.0.0.1:61445/`

      Use that dynamic port number to see the exact model names using this command:

      ```bash
      foundry cache list
      ```

      In the results from the cache list command, look for the model Id.  That will be the value that you need to put in your config file, along with the endpoint URL.  For example:

      ```json
         "Phi4": {
           "ModelId": "Phi-4-mini-reasoning-cuda-gpu:3",
           "Endpoint": "http://127.0.0.1:61445/v1"
         }
      ```

      See [https://learn.microsoft.com/en-us/azure/ai-foundry/foundry-local/reference/reference-cli?view=foundry-classic](https://learn.microsoft.com/en-us/azure/ai-foundry/foundry-local/reference/reference-cli?view=foundry-classic) for more CLI commands.

   - Thought: switch to phi-4-mini-reasoning model?
   - [https://techcommunity.microsoft.com/blog/educatordeveloperblog/running-phi-4-locally-with-microsoft-foundry-local-a-step-by-step-guide/4466304](https://techcommunity.microsoft.com/blog/educatordeveloperblog/running-phi-4-locally-with-microsoft-foundry-local-a-step-by-step-guide/4466304)


2. **Download Phi-4 Model**
   - Open Microsoft Foundry Local
   - Go to the "Discover" tab
   - Search for "phi-4"
   - Click on the Phi-4 model (look for "microsoft/phi-4" or similar)
   - Click "Download" button
   - Wait for the model to download (this may take several minutes depending on your internet connection)

3. **Load and Start the Model**
   - Once downloaded, go to the "My Models" tab
   - Select the Phi-4 model
   - Click "Load Model"
   - Configure the server settings:
     - Default port: 1234
     - API style: OpenAI Compatible
   - Click "Start Server"
   - Verify the server is running (you should see a green indicator)

4. **Test the Model (Optional)**
   - Go to the "Playground" tab
   - Try sending a test message to verify the model is working
   - You should receive a response from Phi-4

### Option 2: Using LM Studio

1. **Install LM Studio**
   - Visit: https://lmstudio.ai/
   - Download and install LM Studio for your operating system

2. **Download Phi-4 Model**
   - Open LM Studio
   - Click on the search/download icon
   - Search for "phi-4"
   - Select a quantized version (e.g., Q4_K_M for a good balance of speed and quality)
   - Download the model

3. **Start the Local Server**
   - In LM Studio, go to the "Local Server" tab
   - Load the Phi-4 model
   - Click "Start Server"
   - Note the endpoint URL (typically http://localhost:1234/v1)

### Verifying the Installation

You can verify that Phi-4 is running by opening a browser and navigating to:
```
http://localhost:1234/v1/models
```

You should see a JSON response listing the available models.

## Configuration

### Database Connection

1. Open `appsettings.json` in the project directory
2. Update the connection string to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=DadABase;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**Connection String Examples:**

- **Windows Authentication (Integrated Security):**
  ```
  Server=localhost;Database=DadABase;Integrated Security=true;TrustServerCertificate=true;
  ```

- **SQL Server Authentication:**
  ```
  Server=localhost;Database=DadABase;User Id=your_username;Password=your_password;TrustServerCertificate=true;
  ```

- **Named Instance:**
  ```
  Server=localhost\\SQLEXPRESS;Database=DadABase;Integrated Security=true;TrustServerCertificate=true;
  ```

### Phi-4 Configuration

The default configuration assumes Phi-4 is running on the standard port:

```json
{
  "Phi4": {
    "ModelId": "phi-4",
    "Endpoint": "http://localhost:1234/v1"
  }
}
```

If you're using a different port or endpoint, update these values accordingly.

### User Secrets (Optional)

For enhanced security, you can store sensitive configuration in user secrets:

```bash
cd src/analyzer
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

## Building the Application

1. Navigate to the analyzer directory:
   ```bash
   cd src/analyzer
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the application:
   ```bash
   dotnet build
   ```

## Running the Application

1. **Ensure Prerequisites are Running:**
   - SQL Server is running and accessible
   - Phi-4 model is loaded and server is started (see installation steps above)

2. **Run the Application:**
   ```bash
   cd src/analyzer
   dotnet run
   ```

3. **Monitor Progress:**
   - The application will display a progress bar
   - Each joke will show its processing status
   - A summary report will be displayed at the end

## How It Works

### Processing Flow

1. **Initialization**
   - Connects to the SQL Server database
   - Verifies connection to the Phi-4 local model
   - Loads existing joke categories

2. **For Each Joke:**
   - **Image Description Generation:**
     - Sends the joke to Phi-4 with a specific prompt
     - Phi-4 generates a family-friendly image description
     - Updates the `ImageTxt` field in the database
     - Skips if the joke already has an image description

   - **Category Assignment:**
     - Asks Phi-4 to categorize the joke
     - Checks if suggested categories exist in the database
     - Creates new categories if needed
     - Links the joke to appropriate categories

3. **Summary Report:**
   - Total records processed
   - Successfully updated records
   - Any errors encountered

### Sample Output

```
     ____      _           _                _                     
    |  _ \    | |         / \   _ __   __ _| |_   _ _______ _ __ 
    | |_) |___| |_ ___   / _ \ | '_ \ / _` | | | | |_  / _ \ '__|
    |  __/ _ \ __/ _ \ / ___ \| | | | (_| | | |_| |/ /  __/ |   
    |_| |  __/ || (_) /_/   \_\_| |_|\__,_|_|\__, /___\___|_|   
         \___|\_\\___/                        |___/              

Batch processing jokes with Phi-4 local model

✓ Database connection successful
Found 150 jokes to process

Processing record 1 of 150 - Joke ID: 1
  ✓ Generated image description
  ✓ Assigned to category: Dad Jokes
Processing record 2 of 150 - Joke ID: 2
  ⊘ Image description already exists, skipping
  ✓ Created new category: Puns
  ✓ Assigned to category: Puns
...

╭─────────────────────────────╮
│         Summary            │
├─────────────────────────────┤
│ Total Records: 150         │
│ Successfully Processed: 148│
│ Errors: 2                  │
╰─────────────────────────────╯

Processing complete!
```

## Troubleshooting

### Database Connection Issues

**Error:** "Database connection failed"

**Solutions:**
- Verify SQL Server is running
- Check the connection string in `appsettings.json`
- Ensure you have access to the DadABase database
- Try using SQL Server Management Studio to test the connection

### Phi-4 Connection Issues

**Error:** "Failed to connect to Phi-4"

**Solutions:**
- Verify Microsoft Foundry Local (or LM Studio) is running
- Ensure the Phi-4 model is loaded and server is started
- Check that the endpoint in `appsettings.json` matches your server (default: http://localhost:1234/v1)
- Test the endpoint by visiting http://localhost:1234/v1/models in a browser

### Model Response Issues

**Error:** "Invalid or empty response from Phi-4"

**Solutions:**
- The model might be overloaded; try processing fewer jokes at a time
- Restart the Phi-4 server
- Try a different model quantization if using LM Studio
- Check the Phi-4 server logs for errors

### Performance Optimization

- **Slow Processing:** The Phi-4 model processes one joke at a time. For large databases, this may take a while. Consider:
  - Running overnight for large batches
  - Processing in smaller batches by filtering jokes
  - Using a more powerful GPU for faster inference

## Project Structure

```
src/analyzer/
├── Models/
│   ├── Joke.cs                 # Joke entity model
│   ├── JokeCategory.cs         # Category entity model
│   └── JokeJokeCategory.cs     # Junction table model
├── JokeDbContext.cs            # Entity Framework DbContext
├── Program.cs                  # Main application logic
├── JokeAnalyzer.csproj         # Project file
├── appsettings.json            # Configuration file
├── globalUsings.cs             # Global using statements
└── README.md                   # This file
```

## Technologies Used

- **.NET 10:** Modern C# runtime
- **Entity Framework Core 10:** Database access
- **Microsoft Semantic Kernel:** AI orchestration framework
- **Spectre.Console:** Rich console output formatting
- **Phi-4:** Local AI model for text generation

## Additional Resources

- [Phi-4 Documentation](https://techcommunity.microsoft.com/blog/educatordeveloperblog/running-phi-4-locally-with-microsoft-foundry-local-a-step-by-step-guide/4466304)
- [Microsoft Foundry Local](https://www.microsoft.com/en-us/microsoft-foundry/local)
- [Microsoft Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Spectre.Console Documentation](https://spectreconsole.net/)

## License

Copyright 2025, Luppes Consulting, Inc. All rights reserved.
