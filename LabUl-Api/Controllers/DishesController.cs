using LabUlApi.Data;
using LabUI.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabUlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DishesController(AppDbContext context)
        {
            _context = context;
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

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}