using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TodoListApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Configura el nivel de logging
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Cambiado a Debug para obtener m�s detalles durante el desarrollo

// Configura CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
        policyBuilder
            .WithOrigins("https://tudominio.com") // Cambia "https://tudominio.com" por el dominio que necesites permitir
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configuraci�n de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuraci�n de JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero // Elimina la tolerancia en la validaci�n de expiraci�n
    };
});

// Aseg�rate de que la autorizaci�n se configure despu�s de la autenticaci�n
builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Configuraci�n de Swagger con autenticaci�n Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticaci�n JWT usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Usa la pol�tica CORS definida antes de los puntos de enrutamiento
app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Este debe venir antes de UseAuthorization.
app.UseAuthorization();

app.MapControllers();

// Redirecci�n de la ra�z a Swagger UI
//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/")
//    {
//        context.Response.Redirect("/swagger");
//    }
//    else
//    {
//        await next();
//    }
//});
app.Use(async (context, next) =>
{
    var userClaims = context.User.Claims.Select(c => new { c.Type, c.Value });
    foreach (var claim in userClaims)
    {
        Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
    }

    await next();
});

app.Run();
