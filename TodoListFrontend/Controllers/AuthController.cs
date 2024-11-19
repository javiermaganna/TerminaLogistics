using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TodoListFrontend.Models;

namespace TodoListFrontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Logout()
        {
            // Eliminar la cookie de autenticación
            Response.Cookies.Delete("AuthToken");

            // Redirigir a la página de inicio de sesión
            return RedirectToAction("Login", "Auth");
        }


        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return View(registerModel);

            var client = _clientFactory.CreateClient("TodoApi");
            var content = new StringContent(JsonConvert.SerializeObject(registerModel), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Error al registrar el usuario. Inténtalo de nuevo.";
                return View(registerModel);
            }

            return RedirectToAction("Login");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (!ModelState.IsValid)
                return View(loginModel);

            var client = _clientFactory.CreateClient("TodoApi");
            var content = new StringContent(JsonConvert.SerializeObject(loginModel), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Credenciales inválidas, intenta nuevamente.";
                return View();
            }

            // Leer el token JWT de la respuesta y almacenarlo
            var tokenResponse = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<dynamic>(tokenResponse).token;

            Response.Cookies.Append("AuthToken", (string)token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                Expires = DateTime.Now.AddMinutes(60)
            });

            return RedirectToAction("Tasks", "Tasks");
        }
    }
}
