using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TadidyVeApi.Data;

// 🔹 Créer le builder
var builder = WebApplication.CreateBuilder(args);

// 🔹 Ajoute les services nécessaires pour les controllers
builder.Services.AddControllers();

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Détecte l'environnement depuis la variable ASPNETCORE_ENVIRONMENT
var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
Console.WriteLine($"⚡ ASPNETCORE_ENVIRONMENT = {envName}");

// 🔹 Détermine si on utilise SQLite ou PostgreSQL
//    SQLite si environnement = Local ou UseSqlite = true dans la config
bool useSqlite = envName.Equals("Local", StringComparison.OrdinalIgnoreCase)
                 || builder.Configuration.GetValue<bool>("UseSqlite");

// 🔹 Récupère la connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 Configure le DbContext selon le type de DB
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
        Console.WriteLine("💾 Using SQLite for offline mode");
        options.UseSqlite(connectionString);
    }
    else
    {
        Console.WriteLine("🗄 Using PostgreSQL");
        options.UseNpgsql(connectionString);
    }
});

// 🔹 JWT configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key is missing in configuration!");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

// 🔹 Build app
var app = builder.Build();

// 🔹 Middleware Swagger en développement
if (app.Environment.IsDevelopment() || envName.Equals("Local", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔹 JWT Middleware
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Map controllers
app.MapControllers();

// 🔹 Appliquer les migrations automatiquement au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 🔹 Définir le port depuis la variable d'environnement
var port = Environment.GetEnvironmentVariable("PORT") ?? "5223";

// 🔹 Démarrer l'application
app.Run($"http://0.0.0.0:{port}");