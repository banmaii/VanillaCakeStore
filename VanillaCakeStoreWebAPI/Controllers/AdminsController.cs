using AutoMapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanillaCakeStoreWebAPI.DTO.Admin;

namespace VanillaCakeStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class AdminsController : ControllerBase
    {
        private IMapper _mapper;
        private readonly VanillaCakeStoreContext _context;
        public AdminsController(IMapper mapper, VanillaCakeStoreContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("[action]")]
        public IActionResult GetDashboard()
        {
            decimal totalOrders = _context.OrderDetails.Sum(od => od.UnitPrice * od.Quantity * ((decimal)(1 - od.Discount)));
            var totalCustomer = _context.Customers.Select(c => c.CustomerId).ToList().Count;
            var totalGuest = _context.Customers.Select(c => c.CustomerId).ToList()
                .Except(_context.Accounts.Where(a => a.CustomerId != null).Select(a => a.CustomerId).ToList())
                .ToList().Count;
            DashboardDTO dashboardDTO = new DashboardDTO
            {
                TotalOrders = totalOrders,
                TotalCustomer = totalCustomer,
                TotalGuest = totalGuest
            };
            return Ok(dashboardDTO);
        }

        [HttpGet("[action]")]
        public IActionResult GetStaticOrder(int year)
        {
            var list = _context.Orders.Where(o => o.OrderDate.Value.Year == year);
            var result = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                var orderByMonth = list.Where(o => o.OrderDate != null && o.OrderDate.Value.Month == i).ToList();
                result.Add(orderByMonth.Count);
            }
            return Ok(result);
        }
    }
}
