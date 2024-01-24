using Microsoft.AspNetCore.Mvc;
using System.Data;
using VanillaCakeStore.Models.Category;
using VanillaCakeStore.Services;
using VanillaCakeStoreWebAPI.DTO.Product;
using Newtonsoft.Json;
using VanillaCakeStoreWebAPI.DTO.Authentication;
using VanillaCakeStoreWebAPI.DTO.Order;
using VanillaCakeStoreWebAPI.DTO.Admin;

namespace eBookStore.Controllers
{

    public class AdminController : Controller
    {
        public async Task<ActionResult> Product(ProductSearchView? searchView)
        {
            try
            {
                var categories = await ClientService.GetAllCategory();
                if (categories == null)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                ViewData["categories"] = categories;
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage productResponse = await ClientService.callGetApi("Products/GetAllFilter?categoryId=" + searchView.CategoryId + (searchView.Search != null ? ("&search=" + searchView.Search) : ""), token);

                if (productResponse.IsSuccessStatusCode)
                {
                    string results = productResponse.Content.ReadAsStringAsync().Result;
                    List<ProductDTO> products = JsonConvert.DeserializeObject<List<ProductDTO>>(results);
                    ViewData["products"] = products;

                }
                else if (productResponse.StatusCode.Equals(System.Net.HttpStatusCode.Unauthorized))
                {
                    ClaimDTO claim = await ClientService.GetAccountClaims(token);
                    string refreshToken = HttpContext.Session.GetString(Constants._refreshToken);
                    if (refreshToken == null || claim == null)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    TokenDTO tokenDTO = await ClientService.GetRefreshToken(claim.AccountId, refreshToken);
                    if (tokenDTO != null)
                    {
                        HttpContext.Session.SetString(Constants._accessToken, tokenDTO.AccessToken);
                        HttpContext.Session.SetString(Constants._refreshToken, tokenDTO.RefreshToken);
                        return RedirectToAction(nameof(Product), searchView);
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
                return View("~/Views/Admin/Index.cshtml");
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }
        }

        public async Task<ActionResult> Create()
        {
            try
            {
                var categories = await ClientService.GetAllCategory();
                if (categories == null)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                ViewData["categories"] = categories;
                return View("~/Views/Admin/Create.cshtml");
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        public async Task<ActionResult> Update(int id)
        {
            try
            {
                var categories = await ClientService.GetAllCategory();
                if (categories == null)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                ViewData["categories"] = categories;
                return View("~/Views/Admin/Update.cshtml", await GetProductById(id));
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        public async Task<ActionResult> CreateProduct(ProductAddDTO product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string token = HttpContext.Session.GetString(Constants._accessToken);

                    HttpResponseMessage response = await ClientService.callPostApi("Products/CreateProduct", token, product);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Product));
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
                            return RedirectToAction(nameof(CreateProduct), product);
                        }
                        else
                        {
                            return RedirectToAction("Login", "Authentication");
                        }
                    }
                    else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }

                }
                else
                {
                    var categories = await ClientService.GetAllCategory();
                    if (categories == null)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    ViewData["categories"] = categories;
                    return View("~/Views/Admin/Create.cshtml");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        public async Task<ActionResult> UpdateProduct(ProductEditDTO product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string token = HttpContext.Session.GetString(Constants._accessToken);
                    HttpResponseMessage response = await ClientService.callPutApi("Products/UpdateProduct", token, product);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Product));
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
                            return RedirectToAction(nameof(UpdateProduct), product);
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
                else
                {
                    var categories = await ClientService.GetAllCategory();
                    if (categories == null)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    ViewData["categories"] = categories;
                    return View("~/Views/Admin/Update.cshtml");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage response = await ClientService.callDeleteApi($"Products/DeleteProduct/{id}", token);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Product));
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
                        return RedirectToAction(nameof(DeleteProduct), id);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }


        }
        public async Task<ActionResult> Order(OrderSearchView? searchView)
        {
            try
            {
                string param = "";
                if (searchView.From != null)
                {
                    param += "&from=" + DateTime.Parse(searchView.From.ToString()).ToString("yyyy-MM-dd");
                }
                if (searchView.To != null)
                {
                    param += "&to=" + DateTime.Parse(searchView.To.ToString()).ToString("yyyy-MM-dd");
                }

                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage response = await ClientService.callGetApi("Orders/GetAllOrders?pageIndex=" + searchView.page + "&pageSize=20" + param, token);

                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    PagingOrderDTO orders = JsonConvert.DeserializeObject<PagingOrderDTO>(results);
                    ViewData["orders"] = orders;
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
                        return RedirectToAction(nameof(Order), searchView);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
                {
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }

                ViewData["searchView"] = searchView;
                return View(searchView);
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        public async Task<ActionResult> CancelOrder(int id)
        {
            try
            {
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage response = await ClientService.callPutApi("Orders/CancelOrder?orderId=" + id, token, id);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Order));
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
                        return RedirectToAction(nameof(CancelOrder), id);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
                {
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }



        }
        public async Task<ActionResult> OrderDetail(int id)
        {
            try
            {
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage response = await ClientService.callGetApi("Orders/GetOrderDetail?id=" + id, token);

                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    OrderDTO orderDetail = JsonConvert.DeserializeObject<OrderDTO>(results);
                    ViewData["orderDetail"] = orderDetail;
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
                        return RedirectToAction(nameof(OrderDetail), id);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
                {
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }

                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        public async Task<IActionResult> Dashboard(int year)
        {
            try
            {
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage dashboardResponse = await ClientService.callGetApi("Admins/GetDashboard", token);
                if (dashboardResponse.IsSuccessStatusCode)
                {
                    string results = dashboardResponse.Content.ReadAsStringAsync().Result;
                    DashboardDTO dashboard = JsonConvert.DeserializeObject<DashboardDTO>(results);
                    ViewData["dashboard"] = dashboard;

                }
                else if (dashboardResponse.StatusCode.Equals(System.Net.HttpStatusCode.Unauthorized))
                {
                    ClaimDTO claim = await ClientService.GetAccountClaims(token);
                    string refreshToken = HttpContext.Session.GetString(Constants._refreshToken);
                    TokenDTO tokenView = await ClientService.GetRefreshToken(claim.AccountId, refreshToken);
                    if (tokenView != null)
                    {
                        HttpContext.Session.SetString(Constants._accessToken, tokenView.AccessToken);
                        HttpContext.Session.SetString(Constants._refreshToken, tokenView.RefreshToken);
                        return RedirectToAction(nameof(Dashboard), year);
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

                HttpResponseMessage response = await ClientService.callGetApi("Admins/GetStaticOrder?year=" + year, token);
                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    List<int> orders = JsonConvert.DeserializeObject<List<int>>(results);
                    ViewData["orders"] = results;

                }
                else if (dashboardResponse.StatusCode.Equals(System.Net.HttpStatusCode.Unauthorized))
                {
                    ClaimDTO claim = await ClientService.GetAccountClaims(token);
                    string refreshToken = HttpContext.Session.GetString(Constants._refreshToken);
                    TokenDTO tokenView = await ClientService.GetRefreshToken(claim.AccountId, refreshToken);
                    if (tokenView != null)
                    {
                        HttpContext.Session.SetString(Constants._accessToken, tokenView.AccessToken);
                        HttpContext.Session.SetString(Constants._refreshToken, tokenView.RefreshToken);
                        return RedirectToAction(nameof(Dashboard), year);
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
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

        private async Task<ProductEditDTO> GetProductById(int id)
        {
            HttpResponseMessage productResponse = await ClientService.callGetApi($"Products/GetProduct/{id}");
            if (productResponse.IsSuccessStatusCode)
            {
                string results = productResponse.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<ProductEditDTO>(results);
            }
            else
            {
                Console.WriteLine("Error Calling web API");
                return null;
            }
        }

    }
}
