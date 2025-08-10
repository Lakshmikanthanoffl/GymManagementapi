using GymManagement.Data;
using GymManagement.Interfaces;
using GymManagement.Repositories;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Services Configuration -------------------

// 🔌 Add PostgreSQL DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔄 Register Repositories and Services
builder.Services.AddScoped<IMembersRepository, MembersRepository>();
builder.Services.AddScoped<IMembersService, MembersService>();

// 🌐 Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
// 📦 Add Controllers with Newtonsoft JSON support
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

// 📖 Swagger for API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------- App Pipeline Configuration -------------------

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Standard Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Run the App
app.Run();
