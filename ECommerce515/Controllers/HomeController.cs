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
    
    public IActionResult Index()
    {
        var products = _context.Products.Include(e => e.Category);

        // Filter
        // Join
        // Pagination
        // Others..

        return View(products.ToList());
    }

    public IActionResult Details([FromRoute] int id)
    {
        var product = _context.Products.Include(e => e.Category).Include(e => e.Brand).FirstOrDefault(e => e.ProductId == id);

        if(product is not null)
        {
            var relatedProducts = _context.Products.Where(e => e.CategoryId == product.CategoryId && e.ProductId != product.ProductId).Skip(0).Take(4);

            var topProduct = _context.Products.Where(e => e.ProductId != product.ProductId).OrderByDescending(e => e.Traffic).Skip(0).Take(4);

            var ProductWithRelated = new ProductWithRelatedVM()
            {
                Product = product,
                RelatedProducts = relatedProducts.ToList(),
                TopProduct = topProduct.ToList()
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
