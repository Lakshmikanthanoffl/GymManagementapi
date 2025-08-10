using GymManagement.Data;
using GymManagement.Interfaces;
using GymManagement.Repositories;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Port Binding for Railway -------------------
builder.WebHost.UseUrls("http://+:8080");

// ------------------- Database Connection -------------------
// Build connection string from environment variables (Railway/Neon)
var host = Environment.GetEnvironmentVariable("PGHOST");
var database = Environment.GetEnvironmentVariable("PGDATABASE");
var username = Environment.GetEnvironmentVariable("PGUSER");
var password = Environment.GetEnvironmentVariable("PGPASSWORD");

var connectionString = $"Host={host};Database={database};Username={username};Password={password};SSL Mode=VerifyFull;Trust Server Certificate=false;";

// Debug print environment variables to logs (for verification)
Console.WriteLine($"PGHOST='{host}'");
Console.WriteLine($"PGDATABASE='{database}'");
Console.WriteLine($"PGUSER='{username}'");
Console.WriteLine($"PGPASSWORD is {(string.IsNullOrEmpty(password) ? "missing" : "set")}");

// ----------- Enable dynamic JSON support for Npgsql -----------
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

// Register DbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ------------------- Services Registration -------------------
builder.Services.AddScoped<IMembersRepository, MembersRepository>();
builder.Services.AddScoped<IMembersService, MembersService>();

// ------------------- CORS Policy -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ------------------- Controllers -------------------
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

// ------------------- Swagger -------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------- App Pipeline -------------------
var app = builder.Build();

app.UseCors("AllowAll");

// Enable Swagger UI on all environments
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
