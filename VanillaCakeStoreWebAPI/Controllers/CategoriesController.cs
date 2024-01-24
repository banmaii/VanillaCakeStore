using AutoMapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanillaCakeStoreWebAPI.DTO.Category;

namespace VanillaCakeStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private IMapper _mapper;
        private readonly VanillaCakeStoreContext _context;
        public CategoriesController(IMapper mapper, VanillaCakeStoreContext context)
        {
            _mapper = mapper;
            _context=context;
        }


        [HttpGet("[action]")]
        public async Task<ActionResult<Category>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            if (categories.Count() != 0)
            {
                return Ok(_mapper.Map<List<CategoryDTO>>(categories));
            }
            else
            {
                return NotFound();
            }
        }

    }
}
