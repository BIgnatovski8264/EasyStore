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

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION")!);
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET")!)
            ),
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
    IServiceProvider services = scope.ServiceProvider;
    AppDbContext context = services.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("--- ПОДГОТОВКА НА БАЗАТА ---");

        if (Environment.GetEnvironmentVariable("DROP_DB_ON_RUN") == "1")
        {
            Console.WriteLine("--- ТРИЕНЕ И ПРЕЗАРЕЖДАНЕ НА ТАБЛИЦИ ---");
            await DatabaseHelper.TruncateAllTablesSafeAsync(context);
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();
        }

        Console.WriteLine("--- СТАРТИРАНЕ НА SEED НА ДАННИ ---");
        await DbInitializer.SeedAsync(services);
        Console.WriteLine("--- ВСИЧКО Е ЗАРЕДЕНО УСПЕШНО ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"КРИТИЧНА ГРЕШКА ПРИ СТАРТ: {ex.Message}");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
