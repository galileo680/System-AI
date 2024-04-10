using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SystemAI.Interfaces;
using SystemAI.Services;
using SystemAI.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:7117") // Dostosuj do adresu URL Twojego klienta
              .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



var algorithmsPath = Path.Combine(Directory.GetCurrentDirectory(), "Algorithms");
var fitnessFunctionPath = Path.Combine(Directory.GetCurrentDirectory(), "Functions");

builder.Services.AddSingleton<IAlgorithmService, AlgorithmService>(sp =>
    new AlgorithmService(algorithmsPath, new AlgorithmLoader(), fitnessFunctionPath, new FitnesFunctionLoader()));

builder.Services.AddSingleton<IFitnessFunctionService, FitnessFunctionService>(sp =>
    new FitnessFunctionService(fitnessFunctionPath));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Konfiguracja dla przesy³ania plików
    c.OperationFilter<SwaggerFileUploadOperationFilter>();
});*/

var app = builder.Build();

app.UseCors("MyCorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
