using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using VanillaCakeStore.Models.Authentication;
using VanillaCakeStore.Models.Home;
using VanillaCakeStore.Services;
using VanillaCakeStoreWebAPI.DTO.Product;

namespace eBookStore.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(HomeFilterView filter)
        {
            var categories = await ClientService.GetAllCategory();
            if (categories == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewData["categories"] = categories;
            HttpResponseMessage productHotResponse = await ClientService.callGetApi("Products/GetProductHot?categoryId=" + filter.CategoryId + "&pageIndex=" + filter.pageHot + "&pageSize=4");
            if (productHotResponse.IsSuccessStatusCode)
            {
                string results = productHotResponse.Content.ReadAsStringAsync().Result;
                PagingProductDTO productsHot = JsonConvert.DeserializeObject<PagingProductDTO>(results);
                ViewData["productsHot"] = productsHot;
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
            HttpResponseMessage productsBestSaleResponse = await ClientService.callGetApi("Products/GetProductBestSale?categoryId=" + filter.CategoryId + "&pageIndex=" + filter.pageSale + "&pageSize=4");
            if (productsBestSaleResponse.IsSuccessStatusCode)
            {
                string results = productsBestSaleResponse.Content.ReadAsStringAsync().Result;
                PagingProductDTO productsBestSale = JsonConvert.DeserializeObject<PagingProductDTO>(results);
                ViewData["productsBestSale"] = productsBestSale;
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }

            HttpResponseMessage productNewResponse = await ClientService.callGetApi("Products/GetProductNew?categoryId=" + filter.CategoryId + "&pageIndex=" + filter.pageNew + "&pageSize=4");
            if (productHotResponse.IsSuccessStatusCode)
            {
                string results = productNewResponse.Content.ReadAsStringAsync().Result;
                PagingProductDTO productsNew = JsonConvert.DeserializeObject<PagingProductDTO>(results);
                ViewData["productsNew"] = productsNew;
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
            ViewData["categoryId"] = filter.CategoryId;
            ViewData["pageHot"] = filter.pageHot;
            ViewData["pageSale"] = filter.pageSale;
            ViewData["pageNew"] = filter.pageNew;
            return View();
        }

        public IActionResult Filter(int id, int? pageHot = 1, int? pageSale = 1, int? pageNew = 1)
        {
            HomeFilterView homeFilterView = new HomeFilterView { CategoryId = id, pageHot = pageHot, pageSale = pageSale, pageNew = pageNew };
            return RedirectToAction("Index", "Home", homeFilterView);
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            HttpResponseMessage response = await ClientService.callGetApi($"Products/GetProduct/{id}");
            if (response.IsSuccessStatusCode)
            {
                string results = response.Content.ReadAsStringAsync().Result;
                ProductDTO product = JsonConvert.DeserializeObject<ProductDTO>(results);
                ViewData["product"] = product;
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
            return View();
        }

        public async Task<IActionResult> Products()
        {
            HttpResponseMessage response = await ClientService.callGetApi("Products/GetAllProduct");
            if (response.IsSuccessStatusCode)
            {
                string results = response.Content.ReadAsStringAsync().Result;
                ProductDTO product = JsonConvert.DeserializeObject<ProductDTO>(results);
                ViewData["product"] = product;
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}