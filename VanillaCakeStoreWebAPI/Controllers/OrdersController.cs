using AutoMapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using VanillaCakeStoreWebAPI.DTO.Order;

namespace VanillaCakeStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private IMapper _mapper;
        private readonly VanillaCakeStoreContext _context;
        public OrdersController(IMapper mapper, VanillaCakeStoreContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetCustomerOrder()
        {
            var header = Request.Headers["Authorization"];
            var token = header[0].Split(" ")[1];

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var customerId = jwt.Claims.First(claim => claim.Type == "CustomerId").Value;
            var result = await _context.Orders.Where(o => o.CustomerId.Equals(customerId) && o.RequiredDate != null)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();
            return Ok(_mapper.Map<List<OrderDTO>>(result));
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetCustomerCanceledOrder()
        {
            var header = Request.Headers["Authorization"];
            var token = header[0].Split(" ")[1];

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var customerId = jwt.Claims.First(claim => claim.Type == "CustomerId").Value;
            var result = await _context.Orders.Where(o => o.CustomerId.Equals(customerId) && o.RequiredDate == null)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();
            return Ok(_mapper.Map<List<OrderDTO>>(result));
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAllOrders(DateTime? from = null, DateTime? to = null, int pageIndex = 1, int pageSize = 8)
        {
            if ((from != null && DateTime.MaxValue < from) || (from != null && new DateTime(2000, 1, 1) > from)
                || (to != null && DateTime.MaxValue < to) || (to != null && new DateTime(2000, 1, 1) > to))
            {
                return BadRequest("Invalid DateTime");
            }

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Where(o =>
                            (from == null || o.OrderDate.Value.Date >= from.Value.Date) &&
                            (to == null || o.OrderDate.Value.Date <= to.Value.Date)
                      )
                .ToListAsync();

            int total = orders.Count();
            int totalPages = total % pageSize == 0 ? (total / pageSize) : ((total / pageSize) + 1);
            var value = _mapper.Map<List<OrderAdminDTO>>(
                orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                );

            return Ok(new PagingOrderDTO
            {
                Total = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = totalPages,
                Values = value
            });
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var result = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            return Ok(_mapper.Map<OrderDTO>(result));
        }

        [HttpPut("[action]")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    return BadRequest("Order doesn't existed!");
                }
                else
                {
                    if (order.ShippedDate == null)
                    {
                        order.RequiredDate = null;
                        _context.Update<Order>(order);
                        await _context.SaveChangesAsync();
                        return Ok(_mapper.Map<OrderDTO>(order));
                    }
                    else
                    {
                        return NotFound("Can't cancel this order!");
                    }
                }
            }
            catch (Exception)
            {
                return BadRequest("Error!");
            }

        }
    }
}
