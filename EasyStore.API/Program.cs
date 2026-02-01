using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using EasyStore.API.Extensions;
using EasyStore.API.Middlewares;
using EasyStore.Data;
using EasyStore.Data.Helpers;
using Microsoft.EntityFrameworkCore.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddOpenApi();
builder.Services.AddCustomServices();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

string dbConn = Environment.GetEnvironmentVariable("DB_CONNECTION") 
                ?? builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? "";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(dbConn);
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

string jwtSecret = Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET") ?? "fallback_secret_key_32_chars_long!!";
string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "EasyStore";
string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "EasyStore";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

string port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(int.Parse(port));
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("--- Database initialization starting ---");
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(services);
        Console.WriteLine("--- Database is ready ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Critical error during DB init: {ex.Message}");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseCors("AllowAll");

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
