using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GameCafe.ManagementServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddScoped<IStationManagementService, StationManagementService>();
builder.Services.AddScoped<ISessionSyncService, SessionSyncService>();
builder.Services.AddScoped<IMultiStationBillingService, MultiStationBillingService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowStations", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();   // serves index.html at /
app.UseStaticFiles();    // serves wwwroot/
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCors("AllowStations");
app.MapControllers();

app.Run();
