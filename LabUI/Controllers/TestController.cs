using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LabUI.Controllers
{
    public class TestController : Controller
    {
        public IActionResult ImageUpload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImageUpload(int dishId, IFormFile imageFile)
        {
            ViewBag.DishId = dishId;

            if (imageFile == null || imageFile.Length == 0)
            {
                ViewBag.Error = "Файл не выбран";
                return View();
            }

            try
            {
                using var httpClient = new HttpClient();
                using var formData = new MultipartFormDataContent();
                using var imageStream = imageFile.OpenReadStream();
                using var streamContent = new StreamContent(imageStream);

                formData.Add(streamContent, "image", imageFile.FileName);

                var url = $"https://localhost:7002/api/dishes/{dishId}/image";
                var response = await httpClient.PostAsync(url, formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                ViewBag.Result = $"Статус: {response.StatusCode}\nОтвет: {responseContent}";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка: {ex.Message}";
            }

            return View();
        }
    }
}