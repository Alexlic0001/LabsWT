using LabUI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabUI.Controllers
{
    public class ImageController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ImageController(UserManager<AppUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> GetAvatar()
        {
            // Получаем email из claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return GetDefaultAvatar();
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return GetDefaultAvatar();
            }

            if (user.Avatar != null && user.Avatar.Length > 0)
            {
                // Определяем MIME-тип на основе сигнатуры файла
                var mimeType = GetImageMimeType(user.Avatar);
                return File(user.Avatar, mimeType);
            }

            return GetDefaultAvatar();
        }

        private string GetImageMimeType(byte[] imageData)
        {
            if (imageData.Length < 4)
                return "image/jpeg";

            // Проверяем сигнатуры файлов для определения типа
            if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                return "image/jpeg";

            if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
                return "image/png";

            if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
                return "image/gif";

            if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                return "image/bmp";

            // По умолчанию возвращаем jpeg
            return "image/jpeg";
        }

        private IActionResult GetDefaultAvatar()
        {
            var imagePath = Path.Combine(_env.WebRootPath, "images", "123.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                // Определяем MIME-тип для файла по умолчанию
                var fileExtension = Path.GetExtension(imagePath).ToLower();
                var mimeType = fileExtension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    _ => "image/jpeg"
                };

                return PhysicalFile(imagePath, mimeType);
            }

            // Если файл по умолчанию не найден, возвращаем 404
            return NotFound("Default avatar image not found");
        }
    }
}