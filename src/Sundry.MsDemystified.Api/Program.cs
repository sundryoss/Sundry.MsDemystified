
using Microsoft.Extensions.Azure;
using Sundry.MsDemystified.Api;
using Sundry.MsDemystified.Api.Interface;

var builder = WebApplication.CreateBuilder(args);

var azBlobSettingsOption = builder.Configuration.GetSection(AzBlobSettingsOption.ConfigKey).Get<AzBlobSettingsOption>()!;

builder.Services.AddAzureClients(builder => builder
        .AddBlobServiceClient(azBlobSettingsOption.ConnectionString)
        .WithName(azBlobSettingsOption.ConnectionName));

builder.Services.AddSingleton(azBlobSettingsOption);
builder.Services.AddSingleton<IAzBlobService, AzBlobService>();

var app = builder.Build();

app.MapGet("/DownloadNotOptimized", async (IAzBlobService azBlobService) => Results.Ok(await azBlobService.DownloadFileFromAzBlobNotOptimizedAsync()));
app.MapGet("/DownloadOptimized", async (IAzBlobService azBlobService) => Results.Ok(await azBlobService.DownloadFileFromAzBlobOptimizedAsync()));

app.Run();

public partial class Program { }
