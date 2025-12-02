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

        // ДОБАВЬТЕ ЭТОТ МЕТОД ДЛЯ ТЕГ-ХЕЛПЕРА
        public IActionResult GetImage(string? imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                // Возвращаем изображение по умолчанию
                return GetDefaultImage();
            }

            var imagePath = Path.Combine(_env.WebRootPath, imageName);

            if (!System.IO.File.Exists(imagePath))
            {
                return GetDefaultImage();
            }

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

        // МЕТОД ДЛЯ АВАТАРА (оставьте как есть)
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
                var mimeType = GetImageMimeType(user.Avatar);
                return File(user.Avatar, mimeType);
            }

            return GetDefaultAvatar();
        }

        private IActionResult GetDefaultImage()
        {
            var imagePath = Path.Combine(_env.WebRootPath, "images", "placeholder.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                return PhysicalFile(imagePath, "image/jpeg");
            }
            return NotFound("Image not found");
        }

        private IActionResult GetDefaultAvatar()
        {
            var imagePath = Path.Combine(_env.WebRootPath, "images", "123.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                return PhysicalFile(imagePath, "image/jpeg");
            }
            return NotFound("Default avatar image not found");
        }

        private string GetImageMimeType(byte[] imageData)
        {
            if (imageData.Length < 4)
                return "image/jpeg";

            if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                return "image/jpeg";

            if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
                return "image/png";

            if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
                return "image/gif";

            if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                return "image/bmp";

            return "image/jpeg";
        }
    }
}