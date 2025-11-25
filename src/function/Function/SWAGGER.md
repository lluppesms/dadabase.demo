# Swagger/OpenAPI Documentation

This Azure Function app includes Swagger/OpenAPI support for API documentation and testing.

## Accessing Swagger UI

### Local Development

When running the function app locally, you can access the Swagger UI at:

- **Swagger UI**: `http://localhost:7071/api/swagger/ui`
- **OpenAPI JSON**: `http://localhost:7071/api/openapi/v3.json`
- **OpenAPI YAML**: `http://localhost:7071/api/openapi/v3.yaml`

### Azure Deployment

When deployed to Azure, replace `localhost:7071` with your function app URL:

- **Swagger UI**: `https://{your-function-app}.azurewebsites.net/api/swagger/ui`
- **OpenAPI JSON**: `https://{your-function-app}.azurewebsites.net/api/openapi/v3.json`

## Configuration

The OpenAPI/Swagger configuration is set in:

1. **appsettings.json** - Contains OpenAPI metadata configuration:
   ```json
   {
     "OpenApi": {
       "Version": "v1",
       "Title": "Dad-A-Base Function API",
       "Description": "Azure Functions API for Dad Jokes",
       "ContactName": "Luppes Consulting",
       "ContactUrl": "http://luppes.com",
       "LicenseName": "MIT License",
       "LicenseUrl": "http://luppes.com/license"
     }
   }
   ```

2. **Helpers/SwaggerConfig.cs** - Contains the OpenAPI configuration extension method
   - `AddSwaggerConfiguration()` - Extension method that registers OpenAPI services
   - Reads configuration from appsettings.json
   - Provides default fallback values

3. **Program.cs** - Registers Swagger configuration:
   ```csharp
   services.AddSwaggerConfiguration(configuration);
   ```

4. **local.settings.json** - Contains environment-specific settings:
   - `OpenApi__HideSwaggerUI` - Set to "false" to show Swagger UI
   - `OpenApi__HideDocument` - Set to "false" to show OpenAPI documents
   - `OpenApi__DocTitle` - Custom title for the documentation
   - `OpenApi__DocDescription` - Custom description

5. **Azure App Settings** - Same settings should be configured in the Function App's Application Settings in Azure Portal

## OpenAPI Attributes

The function endpoints use OpenAPI attributes to document the API:

- `[OpenApiOperation]` - Describes the operation (operationId, tags, summary, description)
- `[OpenApiParameter]` - Describes parameters (name, type, location, required)
- `[OpenApiResponseWithBody]` - Describes responses (status code, content type, body type)

## Example

```csharp
[OpenApiOperation(
    operationId: "RandomJoke", 
    tags: new[] { "jokes" }, 
    Summary = "Get Random Joke", 
    Description = "Gets a random dad joke and returns plain text", 
    Visibility = OpenApiVisibilityType.Important)]
[OpenApiResponseWithBody(
    statusCode: HttpStatusCode.OK, 
    contentType: "text/plain", 
    bodyType: typeof(string), 
    Summary = "A Joke", 
    Description = "This returns a joke in plain text")]
[Function("RandomJoke")]
public HttpResponseData GetRandomJoke([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
{
    // Implementation
}
```

## Available Endpoints

- `GET /api/Joke` - Get a random joke (plain text)
- `GET /api/RandomJoke` - Get a random joke (plain text)
- `GET /api/RandomJokeJson` - Get a random joke (JSON object)
- `GET /api/search/{searchTxt}` - Search for jokes
- `GET /api/search/{searchTxt}/{categoryTxt}` - Search jokes by category
- `GET /api/category/{categoryTxt}` - List jokes in a category
- `GET /api/categories` - Get all joke categories

## Code Organization

The Swagger/OpenAPI configuration follows a clean architecture pattern:

- **Program.cs** - Minimal startup configuration, calls extension method
- **SwaggerConfig.cs** - Contains all OpenAPI configuration logic in an extension method
- **appsettings.json** - Stores configuration values
- **globalUsings.cs** - Contains all necessary using statements

This separation keeps the startup code clean and makes the OpenAPI configuration reusable and testable.

## Customization

To customize the OpenAPI metadata, edit the values in `appsettings.json`:
- **Version**: API version number
- **Title**: API title displayed in Swagger UI
- **Description**: API description displayed in Swagger UI
- **ContactName**: Contact person or organization name
- **ContactUrl**: Contact URL
- **LicenseName**: License name
- **LicenseUrl**: License URL

To modify the OpenAPI configuration logic, edit `Helpers/SwaggerConfig.cs`.

## Troubleshooting

If Swagger UI is not accessible:

1. Verify the function app is running
2. Check that `OpenApi__HideSwaggerUI` is set to "false" in local.settings.json
3. Ensure `appsettings.json` is being copied to the output directory
4. Ensure all required NuGet packages are installed:
   - Microsoft.Azure.Functions.Worker.Extensions.OpenApi
   - Microsoft.Azure.WebJobs.Extensions.OpenApi.Core
   - Microsoft.OpenApi

5. Check the function app logs for any errors
6. For Azure deployments, ensure Application Settings are properly configured
