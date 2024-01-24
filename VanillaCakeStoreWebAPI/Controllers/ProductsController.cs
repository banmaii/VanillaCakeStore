using AutoMapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanillaCakeStoreWebAPI.DTO.Product;

namespace VanillaCakeStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductsController : ControllerBase
    {
        private IMapper _mapper;
        private readonly VanillaCakeStoreContext _context;
        public ProductsController(IMapper mapper, VanillaCakeStoreContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<ProductDTO>>> GetAllProduct()
        {
            var result = await _context.Products.Include(p => p.Category).ToListAsync();
            return Ok(_mapper.Map<List<ProductDTO>>(result));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<PagingProductDTO>> GetProductBestSale(int categoryId, int pageIndex = 1, int pageSize = 4)
        {
            var list = await (from orderDetail in _context.OrderDetails
                              group orderDetail by orderDetail.ProductId into product
                              select new
                              {
                                  ProductId = product.Key,
                                  Discount = product.Max(p => p.Discount)
                              }).OrderByDescending(od => od.Discount)
                       .ThenByDescending(od => od.ProductId)
                       .Join(_context.Products, od => od.ProductId, p => p.ProductId, (od, p) => p)
                       .Where(p => categoryId == 0 || p.CategoryId == categoryId)
                       .Include(p => p.Category)
                       .Include(p => p.OrderDetails)
                       .ToListAsync();
            int total = list.Count();
            int totalPages = total % pageSize == 0 ? (total / pageSize) : ((total / pageSize) + 1);
            var value = _mapper.Map<ICollection<ProductDTO>>(
                list
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize));
            return Ok(new PagingProductDTO
            {
                Total = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = totalPages,
                Values = value
            });
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<PagingProductDTO>> GetProductHot(int categoryId = 0, int pageIndex = 1, int pageSize = 4)
        {
            var list = await _context.Products.Where(p => categoryId == 0 || p.CategoryId == categoryId).Include(p => p.Category)
                .Include(p => p.OrderDetails)
                .ToListAsync();
            int total = list.Count();
            int totalPages = total % pageSize == 0 ? (total / pageSize) : ((total / pageSize) + 1);
            var value = _mapper.Map<ICollection<ProductDTO>>(
                list
                .OrderByDescending(p => p.OrderDetails.Count)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize));
            return Ok(new PagingProductDTO
            {
                Total = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = totalPages,
                Values = value
            });
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<PagingProductDTO>> GetProductNew(int categoryId = 0, int pageIndex = 1, int pageSize = 4)
        {
            var list = await _context.Products
                .Where(p => categoryId == 0 || p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
            int total = list.Count();
            int totalPages = total % pageSize == 0 ? (total / pageSize) : ((total / pageSize) + 1);
            var value = _mapper.Map<ICollection<ProductDTO>>(
                list
                .OrderByDescending(p => p.ProductId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize));
            return Ok(new PagingProductDTO
            {
                Total = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = totalPages,
                Values = value
            });
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).Where(p => p.ProductId == id).FirstOrDefaultAsync();
            if (product != null)
            {
                return Ok(_mapper.Map<ProductDTO>(product));
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateProduct(ProductAddDTO product)
        {
            if (ModelState.IsValid)
            {
                var p = _mapper.Map<Product>(product);
                await _context.AddAsync<Product>(p);
                _context.SaveChanges();
                return CreatedAtAction("GetProduct", new { id = (p != null ? p.ProductId : 0) }, p);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateMulti(List<ProductAddDTO> product)
        {
            if (ModelState.IsValid)
            {
                var products = _mapper.Map<List<Product>>(product);
                foreach (var p in products)
                {
                    await _context.AddAsync<Product>(p);
                    _context.SaveChanges();
                }

                return CreatedAtAction("GetProduct", new { id = (products != null ? products.FirstOrDefault() != null ? products.FirstOrDefault().ProductId : 0 : 0) }, products.FirstOrDefault());
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("[action]/{id}")]
        public async Task<ActionResult<string>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                .Where(e => e.ProductId == id).FirstOrDefaultAsync();
                if (product != null)
                {
                    _context.Remove<Product>(product);
                    _context.SaveChanges();
                    return Ok("Product " + id + " deleted!");
                }
                else
                {
                    return BadRequest("Product " + id + " doesn't existed!");
                }
            }
            catch (Exception)
            {
                return BadRequest("Can't delete product " + id + "!");
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateProduct(ProductEditDTO product)
        {
            var pro = await _context.Products.Where(e => e.ProductId == product.ProductId).AsNoTracking().FirstOrDefaultAsync();
            if (pro != null)
            {
                if (ModelState.IsValid)
                {
                    var p = _mapper.Map<Product>(product);
                    _context.Update<Product>(p);
                    _context.SaveChanges();
                    return Ok(p);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return NotFound();
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpGet("[action]")]
        public async Task<ActionResult<ProductDTO>> GetAllFilter(int categoryId, string? search = null)
        {
            if (search == null)
            {
                search = "";
            }
            var products = await _context.Products.Include(p => p.Category)
                .OrderByDescending(p => p.ProductId)
                .Where(p =>
                (categoryId == 0 || p.CategoryId == categoryId)
                && p.ProductName.Contains(search))
                .ToListAsync();

            if (products.Count() != 0)
            {
                return Ok(_mapper.Map<List<ProductDTO>>(products));
            }
            else
            {
                return Ok(new List<ProductDTO>());
            }
        }
    }
}
