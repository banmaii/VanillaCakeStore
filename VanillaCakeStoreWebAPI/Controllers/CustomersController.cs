using AutoMapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using VanillaCakeStoreWebAPI.DTO.Customer;

namespace VanillaCakeStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private IMapper _mapper;
        private readonly VanillaCakeStoreContext _context;
        public CustomersController(IMapper mapper, VanillaCakeStoreContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<ActionResult<List<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "Customer")]
        public async Task<ActionResult<CustomerDTO>> GetCustomer()
        {
            var customerId = GetCustomerID();
            var customer = await _context.Customers.Include(c => c.Accounts).Where(c => c.CustomerId.Equals(customerId)).FirstOrDefaultAsync();
            return Ok(_mapper.Map<CustomerDTO>(customer));
        }

        [HttpPut("[action]")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> UpdateCustomer(CustomerEditDTO customerDTO)
        {
            var customerId = GetCustomerID();
            var customer = await _context.Customers.Where(c => c.CustomerId.Equals(customerId)).AsNoTracking().FirstOrDefaultAsync();
            if (customer != null)
            {
                if (ModelState.IsValid)
                {
                    Customer customerUpdate = _mapper.Map<Customer>(customerDTO);
                    customerUpdate.CustomerId=customerId;
                    _context.Update<Customer>(customerUpdate);

                    Account account = await _context.Accounts.FirstOrDefaultAsync(a => a.CustomerId.Equals(customerId));
                    account.Email = customerDTO.Email;
                    _context.Update<Account>(account);
                    _context.SaveChanges();
                    return Ok(customerUpdate);
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
        private string GetCustomerID()
        {
            var header = Request.Headers["Authorization"];
            var token = header[0].Split(" ")[1];
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.First(claim => claim.Type == "CustomerId").Value;
        }
    }
}
