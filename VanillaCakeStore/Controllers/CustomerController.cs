using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using VanillaCakeStore.Models.Authentication;
using VanillaCakeStore.Services;
using VanillaCakeStoreWebAPI.DTO.Authentication;
using VanillaCakeStoreWebAPI.DTO.Customer;
using VanillaCakeStoreWebAPI.DTO.Order;

namespace eBookStore.Controllers
{
    public class CustomerController : Controller
    {
        public async Task<IActionResult> Info()
        {
            try
            {
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage response = await ClientService.callGetApi("Customers/GetCustomer", token);
                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    CustomerDTO cus = JsonConvert.DeserializeObject<CustomerDTO>(results);
                    ViewData["customerInfo"] = cus;
                    HttpContext.Session.SetString(Constants._cusName, cus.ContactName);
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
                        return RedirectToAction(nameof(Info));
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

        public async Task<IActionResult> Edit()
        {
            try
            {
                string token = HttpContext.Session.GetString(Constants._accessToken);
                HttpResponseMessage response = await ClientService.callGetApi("Customers/GetCustomer", token);
                if (response.IsSuccessStatusCode)
                {
                    string results = response.Content.ReadAsStringAsync().Result;
                    CustomerEditDTO cus = JsonConvert.DeserializeObject<CustomerEditDTO>(results);
                    HttpContext.Session.SetString(Constants._cusName, cus.ContactName);
                    return View("~/Views/Customer/Edit.cshtml", cus);
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
                        return RedirectToAction(nameof(Edit));
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
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }
        }

        public async Task<IActionResult> EditProfile(CustomerEditDTO customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string token = HttpContext.Session.GetString(Constants._accessToken);

                    HttpResponseMessage response = await ClientService.callPutApi("Customers/UpdateCustomer", token, customer);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Info));
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
                            return RedirectToAction(nameof(EditProfile), customer);
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
                    return RedirectToAction(nameof(Edit));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }
        }
        public async Task<IActionResult> Order()
        {
            string token = HttpContext.Session.GetString(Constants._accessToken);
            HttpResponseMessage response = await ClientService.callGetApi("Orders/GetCustomerOrder", token);

            if (response.IsSuccessStatusCode)
            {
                string results = response.Content.ReadAsStringAsync().Result;
                List<OrderDTO> orders = JsonConvert.DeserializeObject<List<OrderDTO>>(results);
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
                    return RedirectToAction(nameof(Order));
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View();
        }

        public async Task<IActionResult> OrderCanceled()
        {

            string token = HttpContext.Session.GetString(Constants._accessToken);
            HttpResponseMessage response = await ClientService.callGetApi("Orders/GetCustomerCanceledOrder", token);

            if (response.IsSuccessStatusCode)
            {
                string results = response.Content.ReadAsStringAsync().Result;
                List<OrderDTO> orders = JsonConvert.DeserializeObject<List<OrderDTO>>(results);
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
                    return RedirectToAction(nameof(OrderCanceled));
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
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View("~/Views/Customer/Order.cshtml");
        }

    }
}
