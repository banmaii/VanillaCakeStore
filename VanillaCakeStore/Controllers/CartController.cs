using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Data;
using Newtonsoft.Json;
using VanillaCakeStore.Services;
using VanillaCakeStoreWebAPI.DTO.Customer;
using VanillaCakeStoreWebAPI.DTO.Authentication;
using VanillaCakeStoreWebAPI.DTO.Order;
using VanillaCakeStoreWebAPI.DTO.Product;

namespace eBookStore.Controllers
{
    public class CartController : Controller
    {
        private string _cartKey = Constants._cartKey;
        public async Task<IActionResult> CustomerCart(string check)
        {
            string token = HttpContext.Session.GetString(Constants._accessToken);
            if (token != null)
            {
                HttpResponseMessage response = await ClientService.callGetApi("Customers/GetCustomer", token);
                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    CustomerDTO cus = JsonConvert.DeserializeObject<CustomerDTO>(results);
                    ViewData["customerInfo"] = cus;
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Unauthorized))
                {
                    ClaimDTO claim = await ClientService.GetAccountClaims(token);
                    string refreshToken = HttpContext.Session.GetString(Constants._refreshToken);
                    TokenDTO tokenView = await ClientService.GetRefreshToken(claim.AccountId, refreshToken);
                    if (tokenView != null)
                    {
                        HttpContext.Session.SetString(Constants._accessToken, tokenView.AccessToken);
                        HttpContext.Session.SetString(Constants._refreshToken, tokenView.RefreshToken);
                        return RedirectToAction(nameof(CustomerCart), check);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            if (check != null)
            {
                if (check.Equals("Fail"))
                {
                    ViewData["error"] = "Order Fail!";
                }
                else
                {
                    ViewData["success"] = "Order Success!";
                }
            }
            ViewData["cart"] = this.GetCustomerCart();
            return View("~/Views/Customer/Cart.cshtml");
        }

        private List<CartItemDTO> GetCustomerCart()
        {
            string jsonCart = HttpContext.Session.GetString(_cartKey);
            if (jsonCart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItemDTO>>(jsonCart);
            }
            return new List<CartItemDTO>();
        }
        private void SaveCartSession(List<CartItemDTO> cart)
        {
            string jsoncart = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(_cartKey, jsoncart);
        }
        private void ClearCart()
        {
            HttpContext.Session.Remove(_cartKey);
        }

        public async Task<IActionResult> AddToCart(int id, int quantity)
        {
            var cart = GetCustomerCart();
            var item = cart.Where(c => c.ProductID == id).FirstOrDefault();
            if (item == null)
            {
                HttpResponseMessage response = await ClientService.callGetApi($"Products/GetProduct/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    ProductDTO product = JsonConvert.DeserializeObject<ProductDTO>(results);
                    item = new CartItemDTO
                    {
                        ProductID = id,
                        ProductName = product.ProductName,
                        Quantity = quantity,
                        UnitPrice = product.UnitPrice,
                    };
                    cart.Add(item);
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            else
            {
                item.Quantity += quantity;
            }
            this.SaveCartSession(cart);
            return RedirectToAction(nameof(CustomerCart));

        }
        public IActionResult RemoveCartItem(int id)
        {
            var cart = GetCustomerCart();
            var item = cart.Where(c => c.ProductID == id).FirstOrDefault();
            if (item == null)
            {
                return RedirectToAction(nameof(CustomerCart));
            }
            cart.Remove(item);
            SaveCartSession(cart);
            return RedirectToAction(nameof(CustomerCart));
        }
        public IActionResult UpdateCartItemQuantity(int id, int quantity)
        {
            var cart = GetCustomerCart();
            var item = cart.Where(c => c.ProductID == id).FirstOrDefault();
            if (item == null)
            {
                return RedirectToAction(nameof(CustomerCart));
            }
            if (quantity > 0)
            {
                item.Quantity = quantity;
                SaveCartSession(cart);
            }
            else
            {
                cart.Remove(item);
                SaveCartSession(cart);
            }
            return RedirectToAction(nameof(CustomerCart));
        }

        public async Task<IActionResult> OrderCart(CustomerDTO customerInfo)
        {
            DateTime now = DateTime.Now;
            CustomerOrderInfoDTO orderCart = new CustomerOrderInfoDTO
            {
                Address = customerInfo.Address,
                CompanyName = customerInfo.CompanyName,
                ContactName = customerInfo.ContactName,
                ContactTitle = customerInfo.ContactTitle,
                RequiredDate = new DateTime(now.Year, now.Month, now.Day + 1),
                Items = this.GetCustomerCart()
            };
            string token = HttpContext.Session.GetString(Constants._accessToken);
            using (var Client = new HttpClient())
            {
                Client.BaseAddress = new Uri(Constants._baseUrl);
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (token != null)
                {
                    Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                }
                HttpResponseMessage response = await Client.PostAsJsonAsync("Carts/OrderCart", orderCart);
                if (response.IsSuccessStatusCode)
                {
                    ClearCart();
                    return RedirectToAction(nameof(CustomerCart), new { check = "Success" });
                }
                else
                {
                    return RedirectToAction(nameof(CustomerCart), new { check = "Fail" });
                }
            }
        }

    }
}
