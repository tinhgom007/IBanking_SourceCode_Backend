using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using src.Data;
using src.Interfaces.IRepositories;
using src.Interfaces.IServices;
using src.Middleware;
using src.Repositories;
using src.Services;
using src.Utils;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

Console.WriteLine("Current Environment: " + builder.Environment.EnvironmentName);
Console.WriteLine("Connection String: " + builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JWT>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<TokenRefreshMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<AuthenticationGrpcServiceImpl>();

app.MapControllers();

app.Run();
