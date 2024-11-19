var builder = WebApplication.CreateBuilder(args);

// Configurar el servicio HttpClient para realizar peticiones a la API
builder.Services.AddHttpClient("TodoApi", client =>
{
    // Especifica la URL base de la API para simplificar las peticiones
    client.BaseAddress = new Uri("https://localhost:7284/api/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Agregar servicios para controladores con vistas y la inyecci�n de dependencias de Razor Pages
builder.Services.AddControllersWithViews();

// Configuraci�n de sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Duraci�n de la sesi�n en minutos
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
app.UseHttpsRedirection();


// Configurar el uso de sesiones y autenticaci�n
app.UseSession();

// Middleware para enrutamiento y archivos est�ticos
app.UseStaticFiles();
app.UseRouting();

// Habilitar autenticaci�n y autorizaci�n
app.UseAuthentication();
app.UseAuthorization();

// Configurar las rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"); // Se asegura de que la vista de Login sea la predeterminada

app.Run();
