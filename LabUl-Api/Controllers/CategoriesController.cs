using LabUlApi.Data;
using LabUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabUlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<ResponseData<IEnumerable<Category>>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            var response = new ResponseData<IEnumerable<Category>>
            {
                Data = categories,
                Success = true
            };

            return response;
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseData<Category>>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new ResponseData<Category>
                {
                    Success = false,
                    ErrorMessage = "Категория не найдена"
                });
            }

            return new ResponseData<Category>
            {
                Data = category,
                Success = true
            };
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest(new ResponseData<Category>
                {
                    Success = false,
                    ErrorMessage = "ID в URL не совпадает с ID в теле запроса"
                });
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound(new ResponseData<Category>
                    {
                        Success = false,
                        ErrorMessage = "Категория не найдена"
                    });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<ResponseData<Category>>> PostCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id },
                new ResponseData<Category>
                {
                    Data = category,
                    Success = true
                });
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new ResponseData<Category>
                {
                    Success = false,
                    ErrorMessage = "Категория не найдена"
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}