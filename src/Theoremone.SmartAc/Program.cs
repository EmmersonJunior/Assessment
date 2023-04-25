using System.Text.Json.Serialization;
using Theoremone.SmartAc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddSmartAcServices(builder.Configuration);
builder.Services.AddOpenApiDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.EnsureDatabaseSetup();
}
app.UseHttpsRedirection();

app.UseOpenApiDocumentation();
app.UseAuthentication();
app.UseAuthorization();

app.MapSmartAcControllers();
app.Run();