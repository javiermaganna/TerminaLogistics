using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using TodoListFrontend.Models;

namespace TodoListFrontend.Controllers
{
    public class TasksController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public TasksController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Tasks()
        {
            ViewData["Controller"] = "Tasks";
            ViewData["Action"] = "Tasks";

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _clientFactory.CreateClient("TodoApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("tasks");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var tareas = JsonConvert.DeserializeObject<List<TareaDTO>>(data);
                return View(tareas);
            }

            ViewBag.ErrorMessage = "No estás autorizado. Vuelve a iniciar sesión.";
            return RedirectToAction("Login", "Auth");
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TareaDTO taskModel)
        {
            if (ModelState.IsValid)
            {
                var token = Request.Cookies["AuthToken"]; // Obtener el token de la cookie
                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.ErrorMessage = "No estás autorizado. Vuelve a iniciar sesión.";
                    return RedirectToAction("Login", "Auth");
                }

                var client = _clientFactory.CreateClient("TodoApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Enviar el token en la solicitud

                var content = new StringContent(JsonConvert.SerializeObject(taskModel), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("tasks", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Tarea creada exitosamente";
                    return RedirectToAction("Tasks");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error al crear la tarea.";
                }
            }
            return View(taskModel);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "No estás autorizado. Vuelve a iniciar sesión.";
                return RedirectToAction("Login", "Auth");
            }

            var client = _clientFactory.CreateClient("TodoApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"tasks/{id}");

            if (response.IsSuccessStatusCode)
            {
                var task = await response.Content.ReadFromJsonAsync<TareaDTO>();
                return View(task);
            }
            else
            {
                ViewBag.ErrorMessage = "Error al obtener la tarea.";
                return RedirectToAction("Tasks");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(TareaDTO taskModel)
        {
            if (ModelState.IsValid)
            {
                var token = Request.Cookies["AuthToken"]; // Obtener el token de la cookie
                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.ErrorMessage = "No estás autorizado. Vuelve a iniciar sesión.";
                    return RedirectToAction("Login", "Auth");
                }

                var client = _clientFactory.CreateClient("TodoApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Enviar el token en la solicitud

                var content = new StringContent(JsonConvert.SerializeObject(taskModel), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"tasks/{taskModel.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Tarea actualizada exitosamente";
                    return RedirectToAction("Tasks");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error al actualizar la tarea.";
                }
            }
            return View(taskModel);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var token = Request.Cookies["AuthToken"]; // Obtener el token de la cookie
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "No estás autorizado. Vuelve a iniciar sesión.";
                return RedirectToAction("Login", "Auth");
            }

            var client = _clientFactory.CreateClient("TodoApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Enviar el token en la solicitud

            var response = await client.DeleteAsync($"tasks/{id}");

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Tarea eliminada exitosamente";
            }
            else
            {
                ViewBag.ErrorMessage = "Error al eliminar la tarea.";
            }

            return RedirectToAction("Tasks");
        }


    }
}
