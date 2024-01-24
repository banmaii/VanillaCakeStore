using AutoMapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using VanillaCakeStoreWebAPI.DTO.Customer;
using VanillaCakeStoreWebAPI.DTO.Order;

namespace VanillaCakeStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private IMapper _mapper;
        private readonly VanillaCakeStoreContext _context;
        public CartsController(IMapper mapper, VanillaCakeStoreContext context)
        {
            _mapper = mapper;
            _context = context;
            testCart.Add(new CartItemDTO
            {
                ProductID = 1,
                ProductName = "Product 1",
                Quantity = 2,
                UnitPrice = 44
            });
            testCart.Add(new CartItemDTO
            {
                ProductID = 2,
                ProductName = "Product 2",
                Quantity = 3,
                UnitPrice = 55
            });
            testCart.Add(new CartItemDTO
            {
                ProductID = 3,
                ProductName = "Product 3",
                Quantity = 4,
                UnitPrice = 66
            });
        }

        private List<CartItemDTO> testCart = new List<CartItemDTO>();

        private string _cartKey = "Cart";

        private List<CartItemDTO> GetCustomerCart()
        {
            string jsonCart = HttpContext.Session.GetString(_cartKey);
            if (jsonCart != null)
            {
                return JsonSerializer.Deserialize<List<CartItemDTO>>(jsonCart);
            }
            return new List<CartItemDTO>();
        }
        private void SaveCartCookie(List<CartItemDTO> cart)
        {
            string jsoncart = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(_cartKey, jsoncart);
        }

        [HttpGet("[action]")]
        public ActionResult<List<CartItemDTO>> GetCart()
        {
            var result = GetCustomerCart();
            return Ok(result);
        }

        [HttpDelete("[action]")]
        public IActionResult DeleteCart()
        {
            HttpContext.Session.Clear();
            return Ok("Success!");
        }
        [HttpPost("[action]")]
        public IActionResult AddToCart(CartItemDTO cartItem)
        {
            var cart = GetCustomerCart();
            var item = cart.Where(c => c.ProductID == cartItem.ProductID).FirstOrDefault();
            if (item == null)
            {
                var product = _context.Products.Where(p => p.ProductId == cartItem.ProductID).FirstOrDefault();
                item = new CartItemDTO
                {
                    ProductID = cartItem.ProductID,
                    ProductName = product.ProductName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.UnitPrice
                };
                cart.Add(item);
            }
            else
            {
                item.Quantity += cartItem.Quantity;
            }
            SaveCartCookie(cart);
            return CreatedAtAction("GetCart", item);
        }
        [HttpDelete("[action]")]
        public IActionResult RemoveCartItem(int id)
        {
            var cart = GetCustomerCart();
            var item = cart.Where(c => c.ProductID == id).FirstOrDefault();
            if (item == null)
            {
                return BadRequest("Removed item does not existed!");
            }
            cart.Remove(item);
            SaveCartCookie(cart);
            return Ok("Remove item successfully!");
        }

        [HttpPut("[action]")]
        public IActionResult UpdateCartItemQuantity(int id, int quantity)
        {
            var cart = GetCustomerCart();
            var item = cart.Where(c => c.ProductID == id).FirstOrDefault();
            if (item == null)
            {
                return BadRequest("Item doesn't existed!");
            }
            if (quantity > 0)
            {
                item.Quantity = quantity;
                SaveCartCookie(cart);
            }
            else
            {
                cart.Remove(item);
                SaveCartCookie(cart);
            }
            return Ok("Update item successfully!");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> OrderCartAsync(CustomerOrderInfoDTO custInfo)
        {
            if (custInfo.Items.Count > 0)
            {
                var header = Request.Headers["Authorization"];
                string customerId = "";
                Customer c;
                if (header.Count > 0)
                {
                    var token = header[0].Split(" ")[1];
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    customerId = jwt.Claims.First(claim => claim.Type == "CustomerId").Value;
                    c = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId.Equals(customerId));
                }
                else
                {
                    Random random = new Random();
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    bool existed = true;
                    while (existed)
                    {
                        customerId = new string(Enumerable.Repeat(chars, 5)
                            .Select(s => s[random.Next(s.Length)]).ToArray());
                        Customer customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId.Equals(customerId));
                        if (customer == null)
                        {
                            existed = false;
                        }
                    }
                    c = new Customer
                    {
                        CustomerId = customerId,
                        CompanyName = custInfo.CompanyName,
                        ContactName = custInfo.ContactName,
                        ContactTitle = custInfo.ContactTitle,
                        Address = custInfo.Address
                    };
                    _context.Customers.Add(c);
                    _context.SaveChanges();
                }

                Order order = new Order
                {
                    OrderDate = DateTime.Now,
                    RequiredDate = custInfo.RequiredDate,
                    CustomerId = c.CustomerId,
                    ShipAddress = c.Address
                };
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                foreach (var item in custInfo.Items)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductID,
                        Quantity = (short)item.Quantity,
                        Discount = 0,
                        UnitPrice = (decimal) item.UnitPrice
                    };
                    await _context.OrderDetails.AddAsync(orderDetail);
                    await _context.SaveChangesAsync();
                }
                return Ok(order);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
