using LabUlApi.Data;
using LabUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LabUlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DishesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/Dishes
        [HttpGet]
        public async Task<ActionResult<ResponseData<ListModel<Dish>>>> GetDishes(
            string? category,
            int pageNo = 1,
            int pageSize = 3)
        {
            // Создать объект результата
            var result = new ResponseData<ListModel<Dish>>();

            // Фильтрация по категории с загрузкой данных категории
            IQueryable<Dish> query = _context.Dishes.Include(d => d.Category);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(d => d.Category.NormalizedName.Equals(category));
            }

            // Подсчет общего количества элементов
            var totalCount = await query.CountAsync();

            // Подсчет общего количества страниц
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Корректировка номера страницы если он превышает общее количество
            if (pageNo > totalPages && totalPages > 0)
            {
                pageNo = totalPages;
            }

            // Получение данных для текущей страницы
            var items = await query
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Создание объекта ProductListModel с нужной страницей данных
            var listData = new ListModel<Dish>()
            {
                Items = items,
                CurrentPage = pageNo,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            // Поместить данные в объект результата
            result.Data = listData;
            result.Success = true;

            // Если список пустой
            if (items.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Нет объектов в выбранной категории";
            }

            return result;
        }

        // GET: api/Dishes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseData<Dish>>> GetDish(int id)
        {
            var dish = await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null)
            {
                return NotFound(new ResponseData<Dish>
                {
                    Success = false,
                    ErrorMessage = "Блюдо не найдено"
                });
            }

            return new ResponseData<Dish>
            {
                Data = dish,
                Success = true
            };
        }

        // PUT: api/Dishes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDish(int id, Dish dish)
        {
            if (id != dish.Id)
            {
                return BadRequest(new ResponseData<Dish>
                {
                    Success = false,
                    ErrorMessage = "ID в URL не совпадает с ID в теле запроса"
                });
            }

            _context.Entry(dish).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(id))
                {
                    return NotFound(new ResponseData<Dish>
                    {
                        Success = false,
                        ErrorMessage = "Блюдо не найдено"
                    });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Dishes
        [HttpPost]
        public async Task<ActionResult<ResponseData<Dish>>> PostDish(Dish dish)
        {
            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDish", new { id = dish.Id },
                new ResponseData<Dish>
                {
                    Data = dish,
                    Success = true
                });
        }

        // DELETE: api/Dishes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound(new ResponseData<Dish>
                {
                    Success = false,
                    ErrorMessage = "Блюдо не найдено"
                });
            }

            _context.Dishes.Remove(dish);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ДОБАВЬТЕ ЭТОТ МЕТОД:
        [HttpPost("{id}/image")]
        public async Task<IActionResult> SaveImage(int id, IFormFile image)
        {
            try
            {
                // Найти блюдо
                var dish = await _context.Dishes.FindAsync(id);
                if (dish == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Блюдо не найдено"
                    });
                }

                // Проверка файла
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Файл не предоставлен"
                    });
                }

                // Проверка типа файла
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Недопустимый формат файла. Разрешены: JPG, PNG, GIF, WebP"
                    });
                }

                // Проверка размера (5MB)
                if (image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Размер файла не должен превышать 5MB"
                    });
                }

                // Путь к папке wwwroot/images
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var imagesPath = Path.Combine(webRootPath, "images");

                // Создать папку если не существует
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    Console.WriteLine($"Создана папка: {imagesPath}");
                }

                // Удалить старое изображение если оно существует
                if (!string.IsNullOrEmpty(dish.Image))
                {
                    try
                    {
                        // Извлекаем имя файла из URL
                        var oldFileName = Path.GetFileName(dish.Image);
                        if (!string.IsNullOrEmpty(oldFileName))
                        {
                            var oldFilePath = Path.Combine(imagesPath, oldFileName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                                Console.WriteLine($"Удален старый файл: {oldFilePath}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при удалении старого файла: {ex.Message}");
                        // Продолжаем выполнение
                    }
                }

                // Генерация уникального имени файла
                var randomName = Path.GetRandomFileName();
                var fileName = Path.ChangeExtension(randomName, extension);
                var filePath = Path.Combine(imagesPath, fileName);

                Console.WriteLine($"Сохранение файла: {filePath}");

                // Сохранение файла
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                Console.WriteLine($"Файл успешно сохранен: {fileName}, размер: {image.Length} байт");

                // Обновление URL изображения в блюде
                // Для локального хранения используем относительный путь
                //dish.Image = $"/images/{fileName}";

                // ИСПОЛЬЗУЙТЕ ЭТО (абсолютный URL к API):
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}"; 
                dish.Image = $"{baseUrl}/images/{fileName}"; 

                await _context.SaveChangesAsync();

                Console.WriteLine($"URL изображения обновлен: {dish.Image}");

                return Ok(new
                {
                    Success = true,
                    Message = "Изображение успешно сохранено",
                    ImageUrl = dish.Image,
                    FileName = fileName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в SaveImage: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Внутренняя ошибка сервера: {ex.Message}"
                });
            }
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}