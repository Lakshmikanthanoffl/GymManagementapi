using GymManagement.Data;
using GymManagement.Interfaces;
using GymManagement.Repositories;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Npgsql;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Port Binding for Railway -------------------
builder.WebHost.UseUrls("http://+:8080");

// ------------------- Database Connection -------------------
// Read environment variables (Railway/Neon)
var host = Environment.GetEnvironmentVariable("PGHOST");
var database = Environment.GetEnvironmentVariable("PGDATABASE");
var username = Environment.GetEnvironmentVariable("PGUSER");
var password = Environment.GetEnvironmentVariable("PGPASSWORD");

// Fallback raw connection string from appsettings.json
var rawFallbackConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString;

if (!string.IsNullOrWhiteSpace(host) &&
    !string.IsNullOrWhiteSpace(database) &&
    !string.IsNullOrWhiteSpace(username) &&
    !string.IsNullOrWhiteSpace(password))
{
    connectionString = $"Host={host};Database={database};Username={username};Password={password};SSL Mode=VerifyFull;Trust Server Certificate=false;";
}
else if (!string.IsNullOrWhiteSpace(rawFallbackConnectionString))
{
    connectionString = rawFallbackConnectionString
        .Replace("${PGHOST}", Environment.GetEnvironmentVariable("PGHOST") ?? "")
        .Replace("${PGDATABASE}", Environment.GetEnvironmentVariable("PGDATABASE") ?? "")
        .Replace("${PGUSER}", Environment.GetEnvironmentVariable("PGUSER") ?? "")
        .Replace("${PGPASSWORD}", Environment.GetEnvironmentVariable("PGPASSWORD") ?? "");

    Console.WriteLine("Using fallback connection string with replaced environment variables.");
}
else
{
    throw new Exception("No valid connection string found. Please set environment variables or provide a fallback connection string.");
}

// Debug print environment variables
Console.WriteLine($"PGHOST='{host}'");
Console.WriteLine($"PGDATABASE='{database}'");
Console.WriteLine($"PGUSER='{username}'");
Console.WriteLine($"PGPASSWORD is {(string.IsNullOrEmpty(password) ? "missing" : "set")}");

// ----------- Enable dynamic JSON support for Npgsql -----------
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

// Register DbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ------------------- Supabase Client -------------------
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:ServiceRoleKey"];
var supabaseClient = new Client(supabaseUrl, supabaseKey);
await supabaseClient.InitializeAsync();
builder.Services.AddSingleton(supabaseClient);

// ------------------- Services Registration -------------------
builder.Services.AddScoped<IMembersRepository, MembersRepository>();
builder.Services.AddScoped<IMembersService, MembersService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

// ✅ Payment Service & Repository
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
