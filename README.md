En "appsettings" del proyecto API "TodoListApp" agregar tus credenciales a la cadena de conexión:
"DefaultConnection": "Server=your_server;Database=TodoListDb;User Id=your_user;Password=your_password;"

Una vez agregada tus credenciales a la cadena de conexion, recuerda ejecutar estos comandos en la "Consola del Administrador de paquetes"
1.- add-migration dbp1
2.- update-database

En "Program" del proyecto API "TodoListApp" cambiar el dominio que necesites permitir:
.WithOrigins("https://tudominio.com") // Cambia "https://tudominio.com" por el dominio que necesites permitir

En "Program" del proyecto MVC "TodoListFrontend" cambiar la URL base de la API para simplificar las peticiones:
 client.BaseAddress = new Uri("https://localhost:7284/api/");
