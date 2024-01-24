using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using VanillaCakeStore.Models.Authentication;
using VanillaCakeStore.Services;
using VanillaCakeStoreWebAPI.DTO.Authentication;

namespace eBookStore.Controllers
{
    public class AuthenticationController : Controller
    {

        public async Task<IActionResult> Login(string error)
        {
            HttpContext.Session.Remove(Constants._isAdmin);
            HttpContext.Session.Remove(Constants._accessToken);
            HttpContext.Session.Remove(Constants._cusName);
            ViewData["error"] = error;
            return View("~/Views/Login.cshtml");
        }

        public async Task<IActionResult> Logout(string token)
        {
            using (var Client = new HttpClient())
            {
                HttpResponseMessage response = await ClientService.callPostApi("Authentications/Logout", token, null);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
        }

        public async Task<ActionResult> LoginAccount(LoginDTO account)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Login.cshtml");
            }
            else
            {
                HttpResponseMessage response = await ClientService.callPostApi("Authentications/Login", account);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    TokenDTO token = JsonConvert.DeserializeObject<TokenDTO>(result);
                    HttpContext.Session.SetString(Constants._accessToken, token.AccessToken);
                    HttpContext.Session.SetString(Constants._refreshToken, token.RefreshToken);

                    ClaimDTO claim = await ClientService.GetAccountClaims(token.AccessToken);
                    if (claim.Role == 1)
                    {
                        HttpContext.Session.SetString(Constants._isAdmin, "true");
                        return RedirectToAction("Product", "Admin");
                    }
                    else
                    {
                        HttpContext.Session.SetString(Constants._isAdmin, "false");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Authentication", new { error = "Your Email or Password isn't correct!!!" });
                }

            }
        }
        public IActionResult Register()
        {
            return View("~/Views/Register.cshtml");
        }

        public async Task<ActionResult> RegistAccount(RegisterDTO RegisterDTO)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Register.cshtml");
            }
            else
            {
                HttpResponseMessage response = await ClientService.callPostApi("Authentications/Register", RegisterDTO);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
        }


        public IActionResult ForgotPassword(string error)
        {
            ViewData["error"] = error;
            return View("~/Views/ForgotPassword.cshtml");
        }

        public async Task<IActionResult> ResetPassword(EmailView view)
        {
            using (var Client = new HttpClient())
            {
                Client.BaseAddress = new Uri("http://localhost:5000/api/");
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await Client.PostAsJsonAsync("Authentications/ForgotPassword?email=" + view.Email, "");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    return RedirectToAction("ForgotPassword", "Authentication", new { error = "Email doesn't exist!" });
                }
            }
        }

        public IActionResult ChangePass(string error)
        {
            ViewData["error"] = error;
            return View("~/Views/Customer/ChangePass.cshtml");
        }

        public async Task<IActionResult> ChangeAccountPassword(ChangePassView changePass)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction(nameof(ChangePass));
                }
                else if (!changePass.NewPassword.Equals(changePass.ConfirmPassword))
                {
                    return RedirectToAction(nameof(ChangePass), new { error = "New password doesn't match confirm password!" });
                }
                else
                {
                    string token = HttpContext.Session.GetString(Constants._accessToken);

                    HttpResponseMessage response = await ClientService.callPostApi("Authentications/ChangePass", token, changePass);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Info", "Customer");
                    }
                    else if (response.StatusCode.Equals(System.Net.HttpStatusCode.Unauthorized))
                    {
                        ClaimDTO claim = await ClientService.GetAccountClaims(token);
                        string refreshToken = HttpContext.Session.GetString(Constants._refreshToken);
                        TokenDTO tokenDTO = await ClientService.GetRefreshToken(claim.AccountId, refreshToken);
                        if (tokenDTO != null)
                        {
                            HttpContext.Session.SetString(Constants._accessToken, tokenDTO.AccessToken);
                            HttpContext.Session.SetString(Constants._refreshToken, tokenDTO.RefreshToken);
                            return RedirectToAction(nameof(ChangeAccountPassword), changePass);
                        }
                        else
                        {
                            return RedirectToAction("Login", "Authentication");
                        }
                    }
                    else if (response.StatusCode.Equals(System.Net.HttpStatusCode.BadRequest))
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        return RedirectToAction(nameof(ChangePass), new { error = error });
                    }
                    else
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Authentication", new { error = Constants.ErrorMessage.SomeThingHappend });
            }

        }

    }
}
