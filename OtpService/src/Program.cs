using src.Interfaces.IRepositories;
using src.Interfaces.IServices;
using src.Repositories;
using src.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

// Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"))
);

// Register repositories & services
builder.Services.AddScoped<src.Utils.Mailer>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();



var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OTP Service V1"));
}

app.UseHttpsRedirection();

app.MapGrpcService<OtpGrpcServiceImpl>();

app.MapControllers();

app.Run();
