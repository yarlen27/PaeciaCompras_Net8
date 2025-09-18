using SharepointAPI_Net8.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Microsoft Graph API - no configuration needed, handled in service

// Register SharePoint service
builder.Services.AddScoped<SharePointService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SharePoint API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

app.Run();
