using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerce515.Models;
using ECommerce515.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ECommerce515.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _context = new();

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    public IActionResult Index(ProductFilterVM productFilterVM, int page = 1)
    {
        const double discount = 50;
        IQueryable<Product> products = _context.Products;
        var categories = _context.Categories;

        // Join
        products = products.Include(e => e.Category);

        // Filter
        if (productFilterVM.ProductName is not null)
        {
            products = products.Where(e => e.Name.Contains(productFilterVM.ProductName));
            //ViewData["ProductName"] = productFilterVM.ProductName;
            ViewBag.ProductName = productFilterVM.ProductName;
        }

        if (productFilterVM.MinPrice is not null)
        {
            products = products.Where(e => e.Price - (e.Price * ((decimal)e.Discount / 100)) >= (decimal)productFilterVM.MinPrice);
            //ViewData["MinPrice"] = productFilterVM.MinPrice;
            ViewBag.MinPrice = productFilterVM.MinPrice;
        }

        if (productFilterVM.MaxPrice is not null)
        {
            products = products.Where(e => e.Price - (e.Price * ((decimal)e.Discount / 100)) <= (decimal)productFilterVM.MaxPrice);
            //ViewData["MaxPrice"] = productFilterVM.MaxPrice;
            ViewBag.MaxPrice = productFilterVM.MaxPrice;
        }

        if (productFilterVM.CategoryId > 0 && productFilterVM.CategoryId <= categories.Count())
        {
            products = products.Where(e => e.CategoryId == productFilterVM.CategoryId);
            //ViewData["CategoryId"] = productFilterVM.CategoryId;
            ViewBag.CategoryId = productFilterVM.CategoryId;
        }

        if (productFilterVM.IsHot)
        {
            products = products.Where(e => e.Discount > discount);
            //ViewData["IsHot"] = productFilterVM.IsHot;
            ViewBag.IsHot = productFilterVM.IsHot;
        }

        // Pagination
        var totalNumberOfPage = Math.Ceiling(products.Count() / 8.0);

        if (page < 0)
            page = 1;

        products = products.Skip((page - 1) * 8).Take(8);
        ViewBag.TotalNumberOfPage = totalNumberOfPage;
        ViewBag.CurrentPage = page;

        // Others..
        ViewData["Categories"] = categories.ToList();

        return View(products.ToList());
    }

    public IActionResult Details([FromRoute] int id)
    {
        var product = _context.Products.Include(e => e.Category).Include(e => e.Brand).FirstOrDefault(e => e.ProductId == id);

        if(product is not null)
        {
            var relatedProducts = _context.Products.Include(e => e.Category).Where(e => e.CategoryId == product.CategoryId && e.ProductId != product.ProductId).Skip(0).Take(4);

            var topProduct = _context.Products.Include(e => e.Category).Where(e => e.ProductId != product.ProductId).OrderByDescending(e => e.Traffic).Skip(0).Take(4);

            var similarProduct = _context.Products.Include(e => e.Category).Where(e=>e.Name.Contains(product.Name) && e.ProductId != product.ProductId).Skip(0).Take(4);

            var ProductWithRelated = new ProductWithRelatedVM()
            {
                Product = product,
                RelatedProducts = relatedProducts.ToList(),
                TopProduct = topProduct.ToList(),
                SimilarProduct = similarProduct.ToList()
            };

            product.Traffic++;
            _context.SaveChanges();

            return View(ProductWithRelated);
        }

        return NotFound();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Welcome()
    {
        int age = 21;
        string name = "Mohamed";
        List<string> skills = ["C++", "C#", "SQL"];

        var result = new PersonalInfoVM { Age = age, Name = name, Skills = skills };

        return View(result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
